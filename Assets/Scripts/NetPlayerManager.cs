using Mirror;
using System;
using UnityEngine;

public class NetPlayerManager : NetworkManager
{
    public GameObject gameManagerPrefab;
    [SerializeField]
    private NetworkGameManager gameManager;

    public static Action PlayerAdded;

    public override void OnStartServer()
    {
        base.OnStartServer();
        gameManager = Instantiate(gameManagerPrefab).GetComponent<NetworkGameManager>();
        gameManager.SetupGame(false, 4, true);

        NetworkServer.Spawn(gameManager.gameObject);

        PlayerAdded += gameManager.CreateLaserPool;
    }

    public override void OnStopServer()
    {
        PlayerAdded -= gameManager.CreateLaserPool;
        base.OnStopServer();
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);

        PlayerAdded?.Invoke();
        conn.identity.name = "Player " + numPlayers;
    }
}
