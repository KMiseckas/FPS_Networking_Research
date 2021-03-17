using Mirror;
using UnityEngine;
using WorkingTitle.Entities.Player;
using WorkingTitle.Networking;

namespace WorkingTitle.GameFlow
{
    [RequireComponent(typeof(GameNetworkManager))]
    /// <summary>
    /// Singleton instance that exists through out the whole game. Manages the creation and initialisation of the starting game flow logic per scene.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        #region Fields
        private static GameManager _GameInstance;

        private static int _ConnectionToServerID;

        private static object _Lock = new object();

        /// <summary>
        /// Reference to the instance of the <see cref="GameMode"/> prefab for this game session. Exists on server only.
        /// </summary>
        [SerializeField]
        [Tooltip("Prefab reference to the game mode that should be started in this game session")]
        private GameMode _GameMode;

        #endregion

        public static GameManager GetInstance
        {
            get
            {
                lock(_Lock)
                {
                    if(_GameInstance == null)
                    {
                        _GameInstance = FindObjectOfType<GameManager>();
                    }

                    return _GameInstance;
                }
            }
        }

        public GameMode GameMode { get => _GameMode; set => _GameMode=value; }

        public static int ConnectionToServerID { get => _ConnectionToServerID; set => _ConnectionToServerID=value; }

        private void OnEnable()
        {
            GameNetworkManager.Client_OnClientConnectedToServer += OnClientConnectedToServer;
            GameNetworkManager.Client_OnClientDisconnectFromServer += OnClientDisconnectedFromServer;
            GameNetworkManager.Client_OnConnectedClientNoLongerReady += OnClientNotReady;
            GameNetworkManager.Client_OnClientSceneChanged += OnClientSceneChanged;

            GameNetworkManager.Server_OnStartHost += OnStartServer;
            GameNetworkManager.Server_OnStopHost += OnStopServer;
        }

        private void OnDisable()
        {
            GameNetworkManager.Client_OnClientConnectedToServer -= OnClientConnectedToServer;
            GameNetworkManager.Client_OnClientDisconnectFromServer -= OnClientDisconnectedFromServer;
            GameNetworkManager.Client_OnConnectedClientNoLongerReady -= OnClientNotReady;
            GameNetworkManager.Client_OnClientSceneChanged -= OnClientSceneChanged;

            GameNetworkManager.Server_OnStartHost -= OnStartServer;
            GameNetworkManager.Server_OnStopHost -= OnStopServer;
        }

        private void Awake()
        {
            if(_GameInstance != null)
            {
                Destroy (this.gameObject);
                return;
            }

            _GameInstance = this;

            DontDestroyOnLoad (this);
        }

        private void OnStartServer()
        {
            if(NetworkServer.active)
            {
                _GameMode = Instantiate(_GameMode);
            }
        }

        private void OnStopServer()
        {

        }

        [Client]
        private void OnClientConnectedToServer(NetworkConnection connection)
        {
            AddNetworkedPlayer(connection);
        }

        [Client]
        private void OnClientSceneChanged(NetworkConnection connection)
        {
            AddNetworkedPlayer(connection);
        }

        [Client]
        private void OnClientNotReady(NetworkConnection connection)
        {

        }

        [Client]
        private void OnClientDisconnectedFromServer(NetworkConnection connection)
        {
            ConnectionToServerID = 0;

            Debug.Log($"Client has disconnected from server with Connection ID [{connection.connectionId}]");
        }

        /// <summary>
        /// Add a networked player object to the client which will also tell the server to add the player.
        /// </summary>
        /// <param name="connection">Connvection of the owner for this player object.</param>
        private void AddNetworkedPlayer(NetworkConnection connection)
        {
            if(ClientScene.ready && ClientScene.localPlayer == null)
            {
                ClientScene.AddPlayer(connection);

                ConnectionToServerID = connection.connectionId;

                Debug.Log($"Client has connected to server with Connection ID [{connection.connectionId}]");
            }
        }
    }
}
