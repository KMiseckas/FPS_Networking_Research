using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WorkingTitle.GameFlow.FSMSystem;

namespace WorkingTitle.GameFlow
{
    public enum DefaultTransitionIDs
    {
        LobbyToPreGame,
        PreGameToMainGame,
        MainGameToPostGame
    }

    public enum DefaultStateIDs
    {
        Lobby,
        PreGame,
        MainGame,
        PostGame
    }

    [RequireComponent(typeof(DefaultFSMCreator))]
    public class DefaultGameMode : GameMode
    {
        private GSService<DefaultTransitionIDs, DefaultStateIDs> _GSService;

        public GSService<DefaultTransitionIDs, DefaultStateIDs> GSService => _GSService;

        protected override void OnEnable()
        {
            base.OnEnable();

            _GSService = GetComponent<DefaultFSMCreator>().CreateGSService();
        }
    }
}
