using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class NetworkGameManager : NetworkBehaviour
{
    [SyncVar] public bool isTeams = false;
    [SyncVar] public int numLives = 3;
    [SyncVar] public bool isLives = true;

    [SerializeField]
    private Transform laserHolder;
    [SerializeField]
    private GameObject projectilePrefab;
    [SerializeField]
    private GameObject[] lasers;
    private int spawnedLaserCount = 0;

    [Server]
    public void SetupGame(bool teams, int lives, bool gamemode)
    {
        isTeams = teams;
        numLives = lives;
        isLives = gamemode;

    }

    private void Update()
    {
        if (authority)
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                //SpawnProjectile();
            }
        }
    }

    [ClientRpc]
    public void SpawnProjectile()
    {
        if (spawnedLaserCount < lasers.Length)
        {
            lasers[spawnedLaserCount].SetActive(true);
            Bolt bolt = lasers[spawnedLaserCount].GetComponent<Bolt>();
            if (isServer)
            {
                bolt.isServer = true;
            }
            bolt.FireLaser(2);
            spawnedLaserCount++;
        }
    }

    [ClientRpc]
    public void CreateLaserPool()
    {
        Debug.Log("Create Pool");
    }
}
