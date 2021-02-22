using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace WorkingTitle.Networking
{
    public class DefaultNetworkManager : NetworkManager
    {
        public delegate void ConnectionEventHandler(NetworkConnection connection);
        public delegate void ServerEventHandler();

        public static event ConnectionEventHandler Server_OnConnectedClientReady;
        public static event ConnectionEventHandler Server_OnAddPlayerRequest;
        public static event ConnectionEventHandler Server_OnClientDisconnectFromServer;

        public static event ConnectionEventHandler Client_OnClientConnectedToServer;
        public static event ConnectionEventHandler Client_OnConnectedClientNoLongerReady;
        public static event ConnectionEventHandler Client_OnClientDisconnectFromServer;

        public static event ServerEventHandler Server_OnStartHost;
        public static event ServerEventHandler Server_OnStopHost;

        public override void OnServerAddPlayer(NetworkConnection conn)
        {
            Server_OnAddPlayerRequest (conn);
        }

        public override void OnServerDisconnect(NetworkConnection conn)
        {
            Server_OnClientDisconnectFromServer (conn);
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
            if(!clientLoadedScene)
            {
                if(!ClientScene.ready)
                {
                    Client_OnClientConnectedToServer (conn);
                }
            }
        }

        public override void OnClientDisconnect(NetworkConnection conn)
        {
            base.OnClientDisconnect (conn);

            Client_OnClientDisconnectFromServer (conn);
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
