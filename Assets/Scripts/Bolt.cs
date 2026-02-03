using Mirror;
using System.Collections;
using UnityEngine;

public class Bolt : MonoBehaviour
{
    [SerializeField]
    private GameObject laserSectionPrefab;
    [SerializeField]
    public int pID = -1;
    [SerializeField]
    public bool isServer = false;

    public void FireLaser(int playerID)
    {
        pID = playerID;
        if (isServer)
            name = "Server: " + name;
        StartCoroutine(SpawnSection(1, 3));
    }

    public IEnumerator SpawnSection(float spawnDelay, int spawnCount)
    {
        int haveSpawned = 0;
        Transform boltParent = transform;
        while (haveSpawned < spawnCount)
        {
            yield return new WaitForSeconds(spawnDelay);
            boltParent = Instantiate(laserSectionPrefab, boltParent).transform;
            boltParent.name = "Laser " + pID;
            if (isServer)
            {
                boltParent.name += ": Server";
            }
            haveSpawned++;
        }
        gameObject.SetActive(false);
    }
}
