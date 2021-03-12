using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace WorkingTitle.GameFlow.FSMSystem
{
    public abstract class FSMCreator<T, U> : MonoBehaviour
        where T : struct, IConvertible
        where U : struct, IConvertible
    {
        [Serializable]
        struct TransitionMapperStruct
        {
            public T ID;
            public NetworkedTransition NetworkedTransitionPrefab;
            public U SourceGameState;
            public U TargetGameState;
        }

        [Serializable]
        struct GameStateMapper
        {
            public U ID;
            public NetworkedGameState NetworkedGameState;
        }

        [SerializeField]
        private TransitionMapperStruct[] _TransitionMapper;

        [SerializeField]
        private GameStateMapper[] _GameStateMapper;

        [SerializeField]
        private U _InitialGameStateID;

        private GSService<T,U> _GSService;

        public U InitialGameStateID => _InitialGameStateID;

        public GSService<T, U> GSService => _GSService;

        private void Awake()
        {
            SetupGSService();
        }

        private void SetupGSService()
        {
            Debug.Log("GSService initialisation started.");

            _GSService = new GSService<T, U>(InitialGameStateID);

            AddTransitions();
            AddGameStates();

            Debug.Log("GSService initialisation complete.");
        }

        private void AddTransitions()
        {
            for(int i = 0; i < _TransitionMapper.Length; i++)
            {
                Assert.IsFalse(_GSService.HasTransitionByID(_TransitionMapper[i].ID), $"Transition for ID [{_TransitionMapper[i].ID}] is already defined!");

                TransitionMapperStruct data = _TransitionMapper[i];

                Transition<T, U> transition = new Transition<T, U>(data.ID, data.SourceGameState, data.TargetGameState, data.NetworkedTransitionPrefab);

                _GSService.AddTransition(transition);
            }
        }

        private void AddGameStates()
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
