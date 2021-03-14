using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace WorkingTitle.GameFlow.FSMSystem
{
    /// <summary>
    /// Creates a `Finite State Machine` using the given enums for IDs to use for transitions and game states.
    /// </summary>
    /// <typeparam name="T">Enum type to use as an ID for transitions.Str</typeparam>
    /// <typeparam name="U">Enum type to use as an ID for game states.</typeparam>
    public abstract class FSMCreator<T, U> : MonoBehaviour
        where T : struct, IConvertible
        where U : struct, IConvertible
    {
        /// <summary>
        /// A struct that holds the data required to create a transition between source and target game states, that can also be used with `Mirror` network framework.
        /// </summary>
        [Serializable]
        struct TransitionMapperStruct
        {
            /// <summary>
            /// Unique ID to link the transition to.
            /// </summary>
            public T ID;

            /// <summary>
            /// Prefab that will hold the transition logic, this will be spawned on server and replicated across to all clients.
            /// </summary>
            public NetworkedTransition NetworkedTransitionPrefab;

            /// <summary>
            /// The game state which to transition from.
            /// </summary>
            public U SourceGameState;

            /// <summary>
            /// The game state which to transition towards.
            /// </summary>
            public U TargetGameState;
        }

        /// <summary>
        /// Struct that holds the data required for game state initialization.
        /// </summary>
        [Serializable]
        struct GameStateMapper
        {
            /// <summary>
            /// Unique ID to link the game state to.
            /// </summary>
            public U ID;

            /// <summary>
            /// Prefab that will hold the game state logic. this will be spawned on the server and replicated across to all clients.
            /// </summary>
            public NetworkedGameState NetworkedGameState;
        }

        /// <summary>
        /// The array of transitions and their data linked.
        /// </summary>
        [SerializeField]
        private TransitionMapperStruct[] _TransitionMapper;

        /// <summary>
        /// The array of game states and their data linked.
        /// </summary>
        [SerializeField]
        private GameStateMapper[] _GameStateMapper;

        /// <summary>
        /// The game state to initialise on creation of FSM.
        /// </summary>
        [SerializeField]
        private U _InitialGameStateID;

        /// <summary>
        /// The instance of <see cref="GSService{T, U}"/> that was created by this creator.
        /// </summary>
        private GSService<T,U> _GSService;

        public U InitialGameStateID => _InitialGameStateID;

        /// <summary>
        /// Create a instance of <see cref="GSService{T, U}"/>.
        /// </summary>
        /// <returns></returns>
        public GSService<T,U> CreateGSService()
        {
            Assert.IsTrue(_GSService == null, $"Game State Service has already been created for this instance of FSMCreator: [{this.GetType().Name}]");

            SetupGSService();

            return _GSService;
        }

        /// <summary>
        /// Initialise the new instance of service that has been created.
        /// </summary>
        private void SetupGSService()
        {
            Debug.Log($"GSService initialisation started.");

            _GSService = new GSService<T, U>();

            AddTransitionsToService();
            AddGameStatesToService();

            _GSService.Start(InitialGameStateID);

            Debug.Log("GSService initialisation complete.");
        }

        /// <summary>
        /// Add transitions to the new game state service.
        /// </summary>
        private void AddTransitionsToService()
        {
            for(int i = 0; i < _TransitionMapper.Length; i++)
            {
                Assert.IsFalse(_GSService.HasTransitionByID(_TransitionMapper[i].ID), $"Transition for ID [{_TransitionMapper[i].ID}] is already defined!");

                TransitionMapperStruct data = _TransitionMapper[i];

                Transition<T, U> transition = new Transition<T, U>(data.ID, data.SourceGameState, data.TargetGameState, data.NetworkedTransitionPrefab);

                _GSService.AddTransition(transition);
            }
        }

        /// <summary>
        /// Add game states to the new game state service.
        /// </summary>
        private void AddGameStatesToService()
        {
            for(int i = 0; i < _GameStateMapper.Length; i++)
            {
                Assert.IsFalse(_GSService.HasGameStateByID(_GameStateMapper[i].ID), $"Game state for ID [{_GameStateMapper[i].ID}] is already defined!");

                GameStateMapper data = _GameStateMapper[i];

                GameState<U> gameState = new GameState<U>(data.ID, data.NetworkedGameState);

                _GSService.AddGameState(gameState);
            }
        }
    }
}
