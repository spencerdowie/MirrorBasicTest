using Mirror;
using System.Collections;
using UnityEngine;

public class Bolt : MonoBehaviour
{
    [SerializeField]
    private GameObject laserSectionPrefab;
    [SerializeField]
    public int pID = -1;

    public void FireLaser(int playerID, NetworkGameManager gameManager = null)
    {
        pID = playerID;
        if (gameManager != null)
            name = "Server: " + name;
        StartCoroutine(SpawnSection(1, 3));
    }

    public IEnumerator SpawnSection(float spawnDelay, int spawnCount)
    {
        int haveSpawned = 0;
        Transform boltTransform = transform;
        while (haveSpawned < spawnCount)
        {
            yield return new WaitForSeconds(spawnDelay);
            boltTransform = Instantiate(laserSectionPrefab, boltTransform).transform;
            boltTransform.name = "Laser from " + pID;
            boltTransform.GetComponent<BoxCollider>().enabled = true;
            haveSpawned++;
        }
        gameObject.SetActive(false);
    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Collided with " + other.name);
    }
}
