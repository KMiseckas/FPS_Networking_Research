using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace WorkingTitle.Entities.Player
{
    /// <summary>
    /// Handles and holds the information on the connected player. This data is available on all clients and the server.
    /// </summary>
    public abstract class PlayerState : NetworkBehaviour
    {
    }
}
