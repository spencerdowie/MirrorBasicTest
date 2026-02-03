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

    [ClientRpc]
    public void SpawnProjectile(int playerIndex, Vector3 wPos, Quaternion wRot)
    {
        if (spawnedLaserCount < lasers.Length)
        {
            lasers[spawnedLaserCount].SetActive(true);
            lasers[spawnedLaserCount].transform.SetPositionAndRotation(wPos, wRot);
            Bolt bolt = lasers[spawnedLaserCount].GetComponent<Bolt>();
            bolt.FireLaser(playerIndex, isServer ? this : null);
            spawnedLaserCount++;
        }
    }

    [ClientRpc]
    public void AddPlayer(NetworkIdentity identity)
    {
        identity.GetComponent<GamePlayer>().gameManager = this;
    }
}
