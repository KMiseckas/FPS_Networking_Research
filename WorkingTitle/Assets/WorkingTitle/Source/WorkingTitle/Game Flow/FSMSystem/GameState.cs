using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

namespace WorkingTitle.GameFlow.FSMSystem
{
    /// <summary>
    /// A class that handles the entry and exit of a game state across the server and the connected clients. 
    /// Will also replicate a prefab across all clients that will contain the game state logic.
    /// </summary>
    /// <typeparam name="U">Enum used to depict game state IDs</typeparam>
    public class GameState<U>
        where U : struct, IConvertible
    {

        /// <summary>
        /// ID that this game state is associated with.
        /// </summary>
        [SerializeField]
        private readonly U _ID;

        /// <summary>
        /// Prefab that the game state, that upon entry will spawn on every client and the server.
        /// </summary>
        private readonly NetworkedGameState _NetworkedGameState;

        /// <summary>
        /// Instance of prefab that has been spawned on the local machine.
        /// </summary>
        private NetworkedGameState _NetworkedGameStateInstance;

        public U ID => _ID;

        internal NetworkedGameState NetworkedGameState => _NetworkedGameState;

        /// <summary>
        /// Create a new instance of <see cref="GameState{U}"/>.
        /// </summary>
        /// <param name="id">ID to associate this game state with.</param>
        /// <param name="networkedGameState"></param>
        internal GameState(U id, NetworkedGameState networkedGameState)
        {
            _ID = id;
            _NetworkedGameState = networkedGameState;
        }

        /// <summary>
        /// Enter the game state and invoke the enter logic. Will create and spawn the game state prefab across all clients and the server.
        /// </summary>
        internal void Enter()
        {
            Debug.Log($"Entered Game State: {ID}");

            _NetworkedGameStateInstance = UnityEngine.Object.Instantiate(_NetworkedGameState);

            NetworkServer.Spawn(_NetworkedGameStateInstance.gameObject);
        }

        /// <summary>
        /// Exit the game state and invoke the exit logic. Will destroy all instances of game state prefab on all clients and the server.
        /// </summary>
        internal void Exit()
        {
            Debug.Log($"Exited Game State: {ID}");

            NetworkServer.Destroy(_NetworkedGameStateInstance.gameObject);

            UnityEngine.Object.Destroy(_NetworkedGameStateInstance);

            _NetworkedGameStateInstance = null;
        }
    }
}
