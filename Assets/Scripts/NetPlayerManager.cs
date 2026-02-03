using Mirror;
using System;
using UnityEngine;

public class NetPlayerManager : NetworkManager
{
    [SerializeField]
    private NetworkGameManager gameManager;

    public static Action<NetworkIdentity> PlayerAdded;

    public override void OnStartServer()
    {
        base.OnStartServer();
        //NetworkServer.Spawn(gameManager.gameObject);
        gameManager = FindAnyObjectByType<NetworkGameManager>(FindObjectsInactive.Include);
        gameManager.SetupGame(false, 4, true);

        PlayerAdded += gameManager.AddPlayer;
    }

    public override void OnStopServer()
    {
        PlayerAdded -= gameManager.AddPlayer;
        base.OnStopServer();
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);

        PlayerAdded?.Invoke(conn.identity);
        conn.identity.name = "Player " + numPlayers;
    }
}
