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

    public class DefaultFSMCreator : FSMCreator<DefaultTransitionIDs, DefaultStateIDs>
    {

    }
}
