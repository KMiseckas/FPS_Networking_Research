using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorkingTitle.GameFlow.FSMSystem
{
    /// <summary>
    /// Game state that exists on the server as well as being replicated across to all clients.
    /// </summary>
    public abstract class NetworkedGameState : NetworkBehaviour
    {

    }
}
