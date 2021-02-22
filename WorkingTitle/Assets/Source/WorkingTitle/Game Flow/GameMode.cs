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

        /// <summary>
        /// <see cref="GameState"/> in which the player will start upon connection to the game session.
        /// </summary>
        [SerializeField]
        [Tooltip("Prefab reference to the game state to launch and put the players into upon connecting to the server game session")]
        private GameState _StartingGameState;

        /// <summary>
        /// The currently active <see cref="GameState"/> within this game session.
        /// </summary>
        private GameState _CurrentGameState;

        private Dictionary<int, PlayerController> _ControllerDictionary = new Dictionary<int, PlayerController> ();

        private void OnEnable()
        {
            DefaultNetworkManager.Server_OnClientDisconnectFromServer += Server_OnClientDisconnectedFromServer;
            DefaultNetworkManager.Server_OnConnectedClientReady += Server_OnConnectedClientReady;
            DefaultNetworkManager.Server_OnAddPlayerRequest += Server_OnAddPlayerRequest;
        }

        private void OnDisable()
        {
            DefaultNetworkManager.Server_OnClientDisconnectFromServer -= Server_OnClientDisconnectedFromServer;
            DefaultNetworkManager.Server_OnConnectedClientReady -= Server_OnConnectedClientReady;
            DefaultNetworkManager.Server_OnAddPlayerRequest -= Server_OnAddPlayerRequest;
        }

        private void Server_OnAddPlayerRequest(NetworkConnection connection)
        {
            //TODO: change the position and rotation to spawn points
            PlayerController playerController = Instantiate (_PlayerControllerToSpawn, new Vector3 (0, 10, 0), Quaternion.identity);

            playerController.PlayerState = Instantiate (_PlayerStateToSpawn);

            //Link the connection with the player controller. This tells the server which client owns the controller.
            NetworkServer.AddPlayerForConnection (connection, playerController.gameObject);

            _ControllerDictionary.Add (connection.connectionId, playerController);

            StartCoroutine(TrySpawnPlayerEntityFromURL(playerController, connection));
        }

        private void Server_OnClientDisconnectedFromServer(NetworkConnection connection)
        {

        }

        private void Server_OnConnectedClientReady(NetworkConnection connection)
        {
            if (connection.identity == null)
            {
                Debug.Log ("Ready, but no player network identity  present");
            }

            NetworkServer.SetClientReady (connection);
        }

        private void AttachPlayerEntityToPlayerController(PlayerController playerController, PlayerEntity playerEntity)
        {
            playerController.AttachPlayerEntity(playerEntity);

            NetworkServer.Spawn(playerEntity.gameObject);
        }

        private IEnumerator TrySpawnPlayerEntityFromURL(PlayerController playerController, NetworkConnection connection)
        {
            while(playerController.PlayerEntityToSpawnURL == null)
            {
                yield return new WaitForSeconds(0.1f);
            }

            if(!IsPlayerEntityValid(playerController.PlayerEntityToSpawnURL, connection))
            {
                yield break;
            }

            LoadHandleCompleted loadHandleCompleted = new LoadHandleCompleted();

            Addressables.InstantiateAsync(playerController.PlayerEntityToSpawnURL).Completed += loadHandleCompleted.OnPlayerEntityInstantiated;

            while(loadHandleCompleted.PlayerEntityGameObject == null)
            {
                yield return null;
            }

            AttachPlayerEntityToPlayerController(playerController, loadHandleCompleted.PlayerEntityGameObject.GetComponent<PlayerEntity>());
        }

        /// <summary>
        /// Check if this player entity is allowed to be spawned in by the connecting player.
        /// </summary>
        /// <returns></returns>
        private bool IsPlayerEntityValid(string playerEntityURL, NetworkConnection connection)
        {
            return true;
        }
    }
}

internal class LoadHandleCompleted
{
    internal GameObject PlayerEntityGameObject;

    internal void OnPlayerEntityInstantiated(AsyncOperationHandle<GameObject> handle)
    {
        PlayerEntityGameObject = handle.Result;
    }
}