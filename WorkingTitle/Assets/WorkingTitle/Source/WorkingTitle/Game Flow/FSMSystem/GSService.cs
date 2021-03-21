using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace WorkingTitle.GameFlow.FSMSystem
{
    /// <summary>
    /// A class that handles the switching of game states using transitions. A game state can be switched by invoking a transition with a specific ID.
    /// </summary>
    /// <typeparam name="T">Enum representing the transition ID's</typeparam>
    /// <typeparam name="U">Enum representing the game state ID's</typeparam>
    public class GSService<T, U>
        where T : struct, IConvertible
        where U : struct, IConvertible
    {
        /// <summary>
        /// The map of transitions to their IDs.
        /// </summary>
        private readonly Dictionary<T, Transition<T,U>> _TransitionMap = new Dictionary<T, Transition<T,U>>();

        /// <summary>
        /// The map of game states to their ID's.
        /// </summary>
        private readonly Dictionary<U, GameState<U>> _GameStateMap = new Dictionary<U, GameState<U>>();

        /// <summary>
        /// The currently active game state.
        /// </summary>
        private GameState<U> _ActiveState;

        public GameState<U> ActiveState { get => _ActiveState; set => _ActiveState=value; }

        /// <summary>
        /// Add a transition to the service.
        /// </summary>
        /// <param name="transition">Instance of transition.</param>
        public void AddTransition(Transition<T,U> transition)
        {
            _TransitionMap.Add(transition.ID, transition);

            Debug.Log($"Added Transition to GSServices: [{transition.ID}]");
        }

        /// <summary>
        /// Add a game state to the service.
        /// </summary>
        /// <param name="gameState">Instance of game state.</param>
        public void AddGameState(GameState<U> gameState)
        {
            _GameStateMap.Add(gameState.ID, gameState);

            Debug.Log($"Added Game State to GSServices: [{gameState.ID}]");
        }

        /// <summary>
        /// Start the service using an initial game state ID.
        /// </summary>
        /// <param name="initialGameStateID"></param>
        public void Start(U initialGameStateID)
        {
            if(_ActiveState == null)
            {
                SetGameState(initialGameStateID);
            }
        }

        /// <summary>
        /// Issue a transition that will eventually trigger a game state switch.
        /// </summary>
        /// <param name="transitionID">ID of transition to issue.</param>
        public void IssueTransition(T transitionID)
        {
            Debug.Log($"Issued Transition in GSServices: [{transitionID}]");

            Assert.IsTrue(_TransitionMap.TryGetValue(transitionID, out Transition<T,U> transitionOut));
            Assert.IsTrue(transitionOut.SourceGameStateID.Equals(_ActiveState.ID));

            transitionOut.Begin(new Action<Transition<T,U>>(OnTransitionEnd));
        }

        /// <summary>
        /// Logic to run once the issued transition that has ended.
        /// </summary>
        /// <param name="transition"> Instance of transition that has ended.</param>
        protected virtual void OnTransitionEnd(Transition<T,U> transition)
        {
            SetGameState(transition.TargetGameStateID);
        }

        /// <summary>
        /// Exit the currently active game state and replace with a new one.
        /// </summary>
        /// <param name="gameStateID">ID of the new game state.</param>
        internal void SetGameState(U gameStateID)
        {
            Assert.IsTrue(_GameStateMap.TryGetValue(gameStateID, out GameState<U> gameStateOut));

            if(_ActiveState != null)
            {
                _ActiveState.Exit();
            }

            _ActiveState = gameStateOut;
            _ActiveState.Enter();
        }

        /// <summary>
        /// Check if the transition with the following ID exists.
        /// </summary>
        /// <param name="id">ID of the transition to look for.</param>
        /// <returns>True if transition exists.</returns>
        public bool HasTransitionByID(T id)
        {
            return _TransitionMap.ContainsKey(id);
        }

        /// <summary>
        /// Check if the game state with the following ID exists.
        /// </summary>
        /// <param name="id">ID of the game state to look for.</param>
        /// <returns>True if game state exists.</returns>
        public bool HasGameStateByID(U id)
        {
            return _GameStateMap.ContainsKey(id);
        }
    }
}
