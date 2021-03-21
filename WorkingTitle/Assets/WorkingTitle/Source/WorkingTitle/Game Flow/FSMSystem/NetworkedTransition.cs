using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorkingTitle.GameFlow.FSMSystem
{
    /// <summary>
    /// Transition that exists on the server as well as being replicated across to all clients.
    /// </summary>
    public abstract class NetworkedTransition : NetworkBehaviour
    {
        /// <summary>
        /// Action to invoke on completion of this transition.
        /// </summary>
        private Action _OnEndCallback;

        public Action OnEndCallback { get => _OnEndCallback; set => _OnEndCallback=value; }

        protected abstract void Update();

        /// <summary>
        /// Call this once the transition has been completed.
        /// </summary>
        protected void End()
        {
            OnEnd();

            _OnEndCallback.Invoke();
            _OnEndCallback = null;
        }

        /// <summary>
        /// Run any logic that needs to be executed on invoke of <see cref="End()"/>.
        /// </summary>
        protected virtual void OnEnd()
        {

        }
    }
}
