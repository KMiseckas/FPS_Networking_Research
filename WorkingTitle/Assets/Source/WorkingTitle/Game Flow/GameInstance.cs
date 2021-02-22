using Mirror;
using UnityEngine;
using WorkingTitle.Entities.Player;
using WorkingTitle.Networking;

namespace WorkingTitle.GameFlow
{
    /// <summary>
    /// Singleton instance that exists through out the whole game. Manages the creation and initialisation of the starting game flow logic per scene.
    /// </summary>
    public class GameInstance : MonoBehaviour
    {
        private static GameInstance _GameInstance;

        private static object _Lock = new object();

        public static GameInstance GetInstance
        {
            get
            {
                lock (_Lock)
                {
                    if (_GameInstance == null)
                    {
                        _GameInstance = FindObjectOfType<GameInstance> ();
                    }

                    return _GameInstance;
                }
            }
        }

        /// <summary>
        /// Reference to the instance of the <see cref="GameMode"/> prefab for this game session. Exists on server only.
        /// </summary>
        [SerializeField]
        [Tooltip("Prefab reference to the game mode that should be started in this game session")]
        private GameMode _GameMode;

        private void OnEnable()
        {
            DefaultNetworkManager.Client_OnClientConnectedToServer += Client_OnClientConnectedToServer;
            DefaultNetworkManager.Client_OnClientDisconnectFromServer += Client_OnClientDisconnectedFromServer;
            DefaultNetworkManager.Client_OnConnectedClientNoLongerReady += Client_OnClientNotReady;

            DefaultNetworkManager.Server_OnStartHost += OnStartServer;
            DefaultNetworkManager.Server_OnStopHost += OnStopServer;
        }

        private void OnDisable()
        {
            DefaultNetworkManager.Client_OnClientConnectedToServer -= Client_OnClientConnectedToServer;
            DefaultNetworkManager.Client_OnClientDisconnectFromServer -= Client_OnClientDisconnectedFromServer;
            DefaultNetworkManager.Client_OnConnectedClientNoLongerReady -= Client_OnClientNotReady;

            DefaultNetworkManager.Server_OnStartHost -= OnStartServer;
            DefaultNetworkManager.Server_OnStopHost -= OnStopServer;
        }

        private void Awake()
        {
            //This is a singleton object.
            if(_GameInstance == null)
            {
                _GameInstance = this;
            }
            else
            {
                Destroy (this.gameObject);
                return;
            }

            DontDestroyOnLoad (this);
        }

        private void OnStartServer()
        {
            if(NetworkServer.active)
            {
                //TODO apply game mode using prefab URL/Resources in runtime. Currently applying a development game mode in-editor as a reference.
                _GameMode = Instantiate(_GameMode);
            }
        }

        private void OnStopServer()
        {

        }

        private void Client_OnClientConnectedToServer(NetworkConnection connection)
        {
            //TODO we might want to load a scene before setting player as ready, amend this logic later in development to make sure scene is loaded before we set the player as `Ready`.

            //Request the server to initialise the player on the server and clients (as a `PlayerController` instance).
            ClientScene.Ready (connection);
            ClientScene.AddPlayer (connection);
        }

        private void Client_OnClientNotReady(NetworkConnection connection)
        {

        }

        private void Client_OnClientDisconnectedFromServer(NetworkConnection connection)
        {

        }
    }
}
