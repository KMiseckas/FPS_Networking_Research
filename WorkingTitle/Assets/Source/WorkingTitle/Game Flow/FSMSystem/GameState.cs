using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

namespace WorkingTitle.GameFlow.FSMSystem
{
    /// <summary>
    /// Game state handles the flow for a section of the <see cref="GameMode"/> within the current <see cref="GameInstance"/>. The data of this class is visible on all clients and the server.
    /// </summary>
    public class GameState<U>
        where U : struct, IConvertible
    {
        [SerializeField]
        private readonly U _ID;

        private readonly NetworkedGameState _NetworkedGameState;

        private NetworkedGameState _NetworkedGameStateInstance;

        public U ID => _ID;

        internal NetworkedGameState NetworkedGameState => _NetworkedGameState;

        internal GameState(U id, NetworkedGameState networkedGameState)
        {
            _ID = id;
            _NetworkedGameState = networkedGameState;
        }

        internal void Enter()
        {
            Debug.Log($"Entered Game State: {ID}");

            _NetworkedGameStateInstance = UnityEngine.Object.Instantiate(_NetworkedGameState);

            NetworkServer.Spawn(_NetworkedGameStateInstance.gameObject);
        }

        internal void Exit()
        {
            Debug.Log($"Exited Game State: {ID}");

            NetworkServer.Destroy(_NetworkedGameStateInstance.gameObject);

            UnityEngine.Object.Destroy(_NetworkedGameStateInstance);

            _NetworkedGameStateInstance = null;
        }
    }
}
