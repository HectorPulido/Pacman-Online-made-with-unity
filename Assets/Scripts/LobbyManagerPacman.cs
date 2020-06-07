using UnityEngine;
using Mirror;

[AddComponentMenu("")]
public class LobbyManagerPacman : NetworkManager
{

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        base.OnServerAddPlayer(conn);

        if (numPlayers >= maxConnections)
        {
            PacmanManager.singleton.StartPacman();
        }
    }
}
