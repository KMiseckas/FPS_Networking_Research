using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using WorkingTitle.Entities.Player;
using WorkingTitle.Networking;

namespace WorkingTitle.GameFlow
{
    /// <summary>
    /// Abstract class for the handling of the overall rules and properties for the the current game mode being played. This is only created and visible on the server.
    /// </summary>
    public abstract class GameMode : NetworkBehaviour
    {
        #region Fields

        /// <summary>
        /// Reference to the <see cref="PlayerController"/> prefab that should be created for newly connected players.
        /// </summary>
        [SerializeField]
        [Tooltip ("Prefab reference to the player controller to attach the connected user to in this game mode")]
        private PlayerController _PlayerControllerToSpawn;

        /// <summary>
        /// Reference to the <see cref="PlayerState"/> prefab that should be created for the newly created player controller.
        /// </summary>
        [SerializeField]
        [Tooltip("Prefab reference to the player state that should be attached to the newly created player controller.")]
        private PlayerState _PlayerStateToSpawn;

        private readonly Dictionary<int, PlayerController> _ControllerDictionary = new Dictionary<int, PlayerController> ();

        #endregion

        protected virtual void OnEnable()
        {
            GameNetworkManager.Server_OnClientDisconnectFromServer += OnClientDisconnectedFromServer;
            GameNetworkManager.Server_OnConnectedClientReady += OnConnectedClientReady;
            GameNetworkManager.Server_OnAddPlayerRequest += OnAddPlayerRequest;
        }

        protected virtual void OnDisable()
        {
            GameNetworkManager.Server_OnClientDisconnectFromServer -= OnClientDisconnectedFromServer;
            GameNetworkManager.Server_OnConnectedClientReady -= OnConnectedClientReady;
            GameNetworkManager.Server_OnAddPlayerRequest -= OnAddPlayerRequest;
        }

        public override void OnStartServer()
        {

        }

        [Server]
        public PlayerController TryGetPlayerController(int connectionID)
        {
            _ControllerDictionary.TryGetValue(connectionID, out PlayerController controller);

            return controller;
        }

        [Server]
        protected virtual void OnAddPlayerRequest(NetworkConnection connection)
        {
            //TODO: change the position and rotation to spawn points
            PlayerController playerController = Instantiate (_PlayerControllerToSpawn, new Vector3 (0, 10, 0), Quaternion.identity);

            playerController.PlayerState = Instantiate (_PlayerStateToSpawn);

            //Link the connection with the player controller. This tells the server which client owns the controller and also spawns the controller on all clients.
            NetworkServer.AddPlayerForConnection (connection, playerController.gameObject);

            //NetworkServer.Spawn(_PlayerStateToSpawn.gameObject);

            _ControllerDictionary.Add (connection.connectionId, playerController);;
        }

        [Server]
        protected virtual void OnClientDisconnectedFromServer(NetworkConnection connection)
        {
            _ControllerDictionary.TryGetValue(connection.connectionId, out PlayerController playerController);

            NetworkServer.Destroy(playerController.PlayerEntity.gameObject);
            NetworkServer.Destroy(playerController.PlayerState.gameObject);
            NetworkServer.Destroy(playerController.gameObject);

            _ControllerDictionary.Remove(connection.connectionId);
        }

        [Server]
        protected virtual void OnConnectedClientReady(NetworkConnection connection)
        {
            if (connection.identity == null)
            {
                Debug.Log ("Ready, but no player network identity  present");
            }

            NetworkServer.SetClientReady (connection);
        }
    }
}