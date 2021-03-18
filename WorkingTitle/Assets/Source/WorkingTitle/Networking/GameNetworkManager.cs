using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace WorkingTitle.Networking
{
    public class GameNetworkManager : NetworkManager
    {
        public delegate void ConnectionEventHandler(NetworkConnection connection);
        public delegate void ServerSceneChangeHandler(string sceneName);
        public delegate void ServerEventHandler();

        public static event ConnectionEventHandler Server_OnConnectedClientReady;
        public static event ConnectionEventHandler Server_OnAddPlayerRequest;
        public static event ConnectionEventHandler Server_OnClientDisconnectFromServer;

        public static event ConnectionEventHandler Client_OnClientConnectedToServer;
        public static event ConnectionEventHandler Client_OnConnectedClientNoLongerReady;
        public static event ConnectionEventHandler Client_OnClientDisconnectFromServer;
        public static event ConnectionEventHandler Client_OnClientSceneChanged;

        public static event ServerSceneChangeHandler Server_OnServerSceneChanged;

        public static event ServerEventHandler Server_OnStartHost;
        public static event ServerEventHandler Server_OnStopHost;

        public override void Awake()
        {
            autoCreatePlayer = false;

            base.Awake();
        }

        public override void ConfigureServerFrameRate()
        {
            Application.targetFrameRate = serverTickRate;
        }

        public override void OnServerAddPlayer(NetworkConnection conn)
        {
            Server_OnAddPlayerRequest (conn);
        }

        public override void OnServerDisconnect(NetworkConnection conn)
        {
            NetworkServer.DestroyPlayerForConnection(conn);

            Server_OnClientDisconnectFromServer(conn);
        }

        public override void OnServerReady(NetworkConnection conn)
        {
            Server_OnConnectedClientReady (conn);
        }

        public override void OnClientNotReady(NetworkConnection conn)
        {
            Client_OnConnectedClientNoLongerReady (conn);
        }

        public override void OnClientConnect(NetworkConnection conn)
        {
            //Only ready the player on the initial connection if there is no scene that needs to be loaded (the current scene is the correct networked scene).
            if(!clientLoadedScene && !ClientScene.ready)
            {
                ClientScene.Ready(conn);
            }

            Client_OnClientConnectedToServer(conn);
        }

        public override void OnClientDisconnect(NetworkConnection conn)
        {
            base.OnClientDisconnect (conn);

            Client_OnClientDisconnectFromServer (conn);
        }

        public override void OnClientSceneChanged(NetworkConnection conn)
        {
            if(!ClientScene.ready)
            {
                ClientScene.Ready(conn);
            }

            Client_OnClientSceneChanged(conn);
        }

        public override void OnServerSceneChanged(string sceneName)
        {
            base.OnServerSceneChanged(sceneName);

            Server_OnServerSceneChanged(sceneName);
        }

        public override void OnStartServer()
        {
            Server_OnStartHost();
        }

        public override void OnStopServer()
        {
            Server_OnStopHost();
        }
    }
}
