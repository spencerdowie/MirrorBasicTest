using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bolt : NetworkBehaviour
{
    [SerializeField]
    private GameObject laserSectionPrefab;
    [SerializeField]
    private Transform laserPoint;
    [SerializeField]
    private GameObject hitboxPrefab;

    [SerializeField]
    public int PlayerIndex { get; private set; } = -1;
    private int hitLayer = -1;

    const float LaserHeight = 0.4f;
    [field: SerializeField]
    public float LaserSpeed { get; private set; } = 6f;
    [field: SerializeField]
    public float LaserMaxDistance { get; private set; } = 20f;
    [field: SerializeField]
    public int MaxSegments { get; private set; } = 3;
    [field: SerializeField]
    public float LaserLifetime { get; private set; } = 5f;
    [field: SerializeField, Tooltip("Length of the laser trail, -1 for full trail")]
    public float LaserLength { get; private set; } = -1f;
    [SerializeField]
    private Color playerColour;

    private NetworkGameManager gameManager;
    //private bool isServer { get => gameManager != null; }

    public void FireLaser(int playerID, NetworkGameManager gameManager = null)
    {
        PlayerIndex = playerID;
        this.gameManager = gameManager;

        var points = CreatePoints();
        StartCoroutine(MoveLaser(points));
        //StartCoroutine(SpawnSection(1, 3));
    }

    public IEnumerator SpawnSection(float spawnDelay, int spawnCount)
    {
        int haveSpawned = 0;
        Transform boltTransform = transform;
        while (haveSpawned < spawnCount)
        {
            yield return new WaitForSeconds(spawnDelay);
            boltTransform = Instantiate(laserSectionPrefab, boltTransform).transform;
            boltTransform.name = "Laser from " + PlayerIndex;
            boltTransform.GetComponent<BoxCollider>().enabled = isServer;
            haveSpawned++;
        }
        gameObject.SetActive(false);
    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Collided with " + other.name);
    }

    private IEnumerator MoveLaser(Queue<Vector3> points)
    {
        bool hasDest = true;
        float despawnTimer = 0f;
        Vector3 destination = points.Dequeue();
        LaserSegment currentSegment = SpawnSegment(destination);
        while (hasDest && despawnTimer < LaserLifetime)
        {
            yield return null;

            //if (PauseMenu.Instance.IsPaused)
            //    continue;

            despawnTimer += Time.deltaTime;

            float distance = LaserSpeed * Time.deltaTime;
            Vector3 newPos = Vector3.MoveTowards(laserPoint.position, destination, distance);
            //newPos.y = LaserHeight;
            laserPoint.position = newPos;

            currentSegment.UpdateSegment(distance);

            float remainingDistance = Vector3.Distance(laserPoint.position, destination);
            if (remainingDistance <= distance)
            {
                StartCoroutine(currentSegment.ShrinkSegment());
                hasDest = points.TryDequeue(out destination);
                currentSegment = SpawnSegment(destination);
            }
        }
        while (despawnTimer < LaserLifetime)
        {
            //if (!PauseMenu.Instance.IsPaused)
            despawnTimer += Time.deltaTime;
            yield return null;
        }
        yield return new WaitForFixedUpdate();
        DestroyLaser();
    }

    public void DestroyLaser()
    {
        //returnAmmo?.Invoke();
    }

    private LaserSegment SpawnSegment(Vector3 destination)
    {
        LaserSegment segment = Instantiate(hitboxPrefab, transform).GetComponent<LaserSegment>();
        segment.Setup(hitLayer, laserPoint.position, destination, PlayerIndex, LaserSpeed, LaserLength, playerColour);
        return segment;
    }

    private Queue<Vector3> CreatePoints()
    {
        Queue<Vector3> points = new Queue<Vector3>();
        float distanceLeft = LaserMaxDistance;
        int numSegments = 0;
        Vector3 segmentOrigin = laserPoint.position, segmentDir = laserPoint.forward;

        segmentOrigin.y = LaserHeight;
        segmentDir.y = 0;

        //Debug.Log("Laser Origin: " + segmentOrigin.ToString());

        while (distanceLeft > 0 && numSegments < MaxSegments)
        {
            bool missed = !Physics.Raycast(segmentOrigin, segmentDir, out RaycastHit hit, 100, LayerMask.GetMask("Default"));
            if (missed)
            {
                hit.point = segmentOrigin + (segmentDir * distanceLeft);
                hit.distance = LaserMaxDistance;
            }

            Vector3 point = hit.point;
            point.y = LaserHeight;

            Vector3 normal = hit.normal;
            normal.y = 0;

            numSegments++;
            distanceLeft -= hit.distance;
            Debug.DrawLine(segmentOrigin, point, Color.red, 3);
            segmentOrigin = point;
            segmentDir = Vector3.Reflect(segmentDir, normal);
            points.Enqueue(point);
        }
        return points;
    }
}
