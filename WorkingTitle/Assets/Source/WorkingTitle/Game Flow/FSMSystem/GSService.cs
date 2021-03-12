using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace WorkingTitle.GameFlow.FSMSystem
{
    public class GSService<T, U>
        where T : struct, IConvertible
        where U : struct, IConvertible
    {
        private readonly Dictionary<T, Transition<T,U>> _TransitionMap = new Dictionary<T, Transition<T,U>>();

        private readonly Dictionary<U, GameState<U>> _GameStateMap = new Dictionary<U, GameState<U>>();

        private GameState<U> _ActiveState;

        public GameState<U> ActiveState { get => _ActiveState; set => _ActiveState=value; }

        public GSService(U initialGameStateID)
        {
            SetGameState(initialGameStateID);
        }

        public void AddTransition(Transition<T,U> transition)
        {
            _TransitionMap.Add(transition.ID, transition);

            Debug.LogError($"Added Transition to GSServices: [{transition.ID}]");
        }

        public void AddGameState(GameState<U> gameState)
        {
            _GameStateMap.Add(gameState.ID, gameState);

            Debug.LogError($"Added Game State to GSServices: [{gameState.ID}]");
        }

        public void IssueTransition(T transitionID)
        {
            Debug.LogError($"Issued Transition in GSServices: [{transitionID}]");

            Assert.IsTrue(_TransitionMap.TryGetValue(transitionID, out Transition<T,U> transitionOut));
            Assert.IsTrue(transitionOut.SourceGameStateID.Equals(_ActiveState.ID));

            transitionOut.Begin(new Action<Transition<T,U>>(OnTransitionEnd));
        }

        protected virtual void OnTransitionEnd(Transition<T,U> transition)
        {
            SetGameState(transition.TargetGameStateID);
        }

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

        public bool HasTransitionByID(T id)
        {
            return _TransitionMap.ContainsKey(id);
        }

        public bool HasGameStateByID(U id)
        {
            return _GameStateMap.ContainsKey(id);
        }
    }
}
