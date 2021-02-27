using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace WorkingTitle.GameFlow
{
    /// <summary>
    /// Game state handles the flow for a section of the <see cref="GameMode"/> within the current <see cref="GameInstance"/>. The data of this class is visible on all clients and the server.
    /// </summary>
    public abstract class GameState : NetworkBehaviour
    {

    }
}
