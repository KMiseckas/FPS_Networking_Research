using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WorkingTitle.GameFlow.FSMSystem;

namespace WorkingTitle.GameFlow
{
    /// <summary>
    /// Enum for the transition ID's to use in the default game mode.
    /// </summary>
    public enum DefaultTransitionIDs
    {
        LobbyToPreGame,
        PreGameToMainGame,
        MainGameToPostGame
    }

    /// <summary>
    /// Enum for the state ID's to use in the default game mode.
    /// </summary>
    public enum DefaultStateIDs
    {
        Lobby,
        PreGame,
        MainGame,
        PostGame
    }

    /// <summary>
    /// Default game mode.
    /// </summary>
    [RequireComponent(typeof(DefaultFSMCreator))]
    public class DefaultGameMode : GameMode
    {
        //TODO figure out a way to move this into the base class of `GameMode`.
        private GSService<DefaultTransitionIDs, DefaultStateIDs> _GSService;

        public GSService<DefaultTransitionIDs, DefaultStateIDs> GSService => _GSService;

        protected override void OnEnable()
        {
            base.OnEnable();

            _GSService = GetComponent<DefaultFSMCreator>().CreateGSService();
        }
    }
}
