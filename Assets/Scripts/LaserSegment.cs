using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserSegment : MonoBehaviour
{
    private BoxCollider hitbox;
    //private LineRenderer lineRenderer;
    private float length = -1f, speed = 0f;
    private Vector3 startPosition, destination;
    private float distanceTravelled = 0f;
    [SerializeField]
    private Transform laserMesh;


    private void Awake()
    {
        hitbox = GetComponent<BoxCollider>();
        //lineRenderer = GetComponent<LineRenderer>();
    }

    public void Setup(int hitLayer, Vector3 startPosition, Vector3 destination,
        int playerIndex, float speed, float length, Color colour)
    {
        this.startPosition = startPosition;
        this.destination = destination;
        this.speed = speed;
        this.length = length;
        gameObject.layer = hitLayer;
        transform.position = startPosition;
        transform.LookAt(destination);
        name = "Player " + playerIndex + " Laser Hitbox";
        //lightningSystem.SetColour(colour);
    }

    public void UpdateSegment(float distance)
    {
        transform.position = Vector3.MoveTowards(transform.position, destination, distance);
        if (distanceTravelled < length)
        {
            distanceTravelled += distance;
            hitbox.size += Vector3.forward * distance;
            hitbox.center += Vector3.back * (distance / 2f);
            laserMesh.localScale += Vector3.up * distance / 2f;
            laserMesh.localPosition += Vector3.back * (distance / 2f);
            //lineRenderer.SetPosition(1, transform.position - startPosition);
        }
    }

    public IEnumerator ShrinkSegment()
    {
        //Vector3 tailPosition = lineRenderer.GetPosition(1);
        float distance;
        do
        {
            distance = speed * Time.deltaTime;
            if (distanceTravelled < length)
            {
                distanceTravelled += distance;
            }
            else
            {
                hitbox.size -= Vector3.forward * distance;
                hitbox.center += Vector3.forward * (distance / 2f);
                laserMesh.localScale -= Vector3.up * distance / 2f;
                laserMesh.localPosition += Vector3.forward * (distance / 2f);
                //lineRenderer.SetPosition(1, tailPosition);
            }
            yield return null;
        }
        while (hitbox.size.z > distance);
        Destroy(gameObject);
    }
}
