using Mirror;
using UnityEngine;
using WorkingTitle.Entities.Player;
using WorkingTitle.Enums;
using WorkingTitle.Networking;

namespace WorkingTitle.GameFlow
{
    [RequireComponent(typeof(GameNetworkManager))]
    /// <summary>
    /// Singleton instance that exists through out the whole game. Manages the creation and initialisation of the starting game flow logic per scene.
    /// </summary>
    public class GameCore : MonoBehaviour
    {
        private const int MIN_PLAYERS_REQUIRED = 2;
        private const int MAX_PLAYERS_ALLOWED = 8;

        #region Fields
        private static GameCore _GameInstance;

        private static int _ConnectionToServerID;

        private static object _Lock = new object();

        /// <summary>
        /// Reference to the instance of the <see cref="GameMode"/> instance for this game session. Exists on server only.
        /// </summary>
        private GameMode _GameMode;

        /// <summary>
        /// Reference to the instance of the <see cref="GameMode"/> prefab for this game session. Exists on server only.
        /// </summary>
        [SerializeField]
        [Tooltip("Prefab reference to the game mode that should be started in this game session")]
        private GameMode _GameModePrefab;

        /// <summary>
        /// Singleton instance of this games network manager.
        /// </summary>
        private GameNetworkManager _NetworkManager;

        #endregion

        public static GameCore Instance
        {
            get
            {
                lock(_Lock)
                {
                    if(_GameInstance == null)
                    {
                        _GameInstance = FindObjectOfType<GameCore>();
                    }

                    return _GameInstance;
                }
            }
        }

        public GameMode GameMode { get => _GameMode; set => _GameMode=value; }

        public static int ConnectionToServerID { get => _ConnectionToServerID; set => _ConnectionToServerID=value; }
        public GameNetworkManager NetworkManager => _NetworkManager;

        private void OnEnable()
        {
            GameNetworkManager.Client_OnClientConnectedToServer += OnClientConnectedToServer;
            GameNetworkManager.Client_OnClientDisconnectFromServer += OnClientDisconnectedFromServer;
            GameNetworkManager.Client_OnConnectedClientNoLongerReady += OnClientNotReady;
            GameNetworkManager.Client_OnClientSceneChanged += OnClientSceneChanged;

            GameNetworkManager.Server_OnServerSceneChanged += OnServerSceneChanged;

            GameNetworkManager.Server_OnStartHost += OnStartServer;
            GameNetworkManager.Server_OnStopHost += OnStopServer;
        }

        private void OnDisable()
        {
            GameNetworkManager.Client_OnClientConnectedToServer -= OnClientConnectedToServer;
            GameNetworkManager.Client_OnClientDisconnectFromServer -= OnClientDisconnectedFromServer;
            GameNetworkManager.Client_OnConnectedClientNoLongerReady -= OnClientNotReady;
            GameNetworkManager.Client_OnClientSceneChanged -= OnClientSceneChanged;

            GameNetworkManager.Server_OnServerSceneChanged -= OnServerSceneChanged;

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

            DontDestroyOnLoad(this);

            _NetworkManager = GetComponent<GameNetworkManager>();
        }

        private void OnStartServer()
        {
            Debug.Log("Server has started.");

            if(NetworkServer.active && (_NetworkManager.onlineScene == null || _NetworkManager.onlineScene == ""))
            {
                if(_GameMode == null)
                {
                    _GameMode = Instantiate(_GameModePrefab);
                }

                Debug.Log("Game mode has been created!");
            }
        }

        private void OnStopServer()
        {

        }

        /// <summary>
        /// Join a server/host with a specific ip address.
        /// </summary>
        /// <param name="ipAddress">IP address of the server/host to join.</param>
        public void Join(string ipAddress)
        {
            NetworkManager.networkAddress = ipAddress;
            NetworkManager.StartClient();
        }

        /// <summary>
        /// Host a server and be a client.
        /// </summary>
        /// <param name="minPlayers">Minimum amount of players before the game can be started.</param>
        /// <param name="maxPlayers">Maximum amount of players allowed.</param>
        /// <param name="mapData">The data of the map to load up.</param>
        public void Host(int minPlayers, int maxPlayers, MapData mapData)
        {
            NetworkManager.maxConnections = Mathf.Min(maxPlayers, MAX_PLAYERS_ALLOWED);//TODO replace with other logic later down the line.

            NetworkManager.onlineScene = mapData.Scene;
            NetworkManager.StartHost();
        }

        /// <summary>
        /// Launch a server instance.
        /// </summary>
        /// <param name="minPlayers">Minimum amount of players before the game can be started.</param>
        /// <param name="maxPlayers">Maximum amount of players allowed.</param>
        /// <param name="mapData">The data of the map to load up.</param>
        public void StartServer(int minPlayers, int maxPlayers, MapData mapData)
        {
            NetworkManager.maxConnections = Mathf.Min(maxPlayers, MAX_PLAYERS_ALLOWED);//TODO replace with other logic later down the line.

            NetworkManager.onlineScene = mapData.Scene;
            NetworkManager.StartServer();
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

        [Server]
        private void OnServerSceneChanged(string sceneName)
        {
            Debug.Log("Server has changed scenes!");

            if(NetworkServer.active)
            {
                _GameMode = Instantiate(_GameModePrefab);

                Debug.Log("Game mode has been created!");
            }
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
