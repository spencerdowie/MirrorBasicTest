using Mirror;
using System.Collections;
using System.Collections.Generic;
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
    private GameObject[] bolts = new GameObject[10];
    [SerializeField, SyncVar]
    private int[] playerAmmo;

    private int tempCount = 0;

    [Server]
    public void SetupGame(bool teams, int lives, bool gamemode)
    {
        isTeams = teams;
        numLives = lives;
        isLives = gamemode;

        playerAmmo = new int[2];//numplayers
        for (int i = 0; i < 2; i++)
        {
            playerAmmo[i] = 3;
        }
    }

    private void OnEnable()
    {
        StartCoroutine(StartGame());
    }

    //Workaround for mirror auto-enabling netIdents on load
    public IEnumerator StartGame()
    {
        yield return new WaitForEndOfFrame();
        foreach (GameObject bolt in bolts)
        {
            bolt.SetActive(false);
        }

    }

    [ClientRpc]
    public void SpawnProjectile(int playerIndex, Vector3 wPos, Quaternion wRot)
    {
        Bolt bolt = bolts[tempCount].GetComponent<Bolt>();
        bolt.gameObject.SetActive(true);
        bolt.transform.SetPositionAndRotation(wPos, wRot);
        bolt.FireLaser(playerIndex, isServer ? this : null);
        tempCount++;
    }


    [ClientRpc]
    public void AddPlayer(NetworkIdentity identity)
    {
        identity.GetComponent<GamePlayer>().gameManager = this;
    }
}
