using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorkingTitle.GameFlow.FSMSystem
{
    public abstract class NetworkedTransition : NetworkBehaviour
    {
        private Action _OnEndCallback;

        public Action OnEndCallback { get => _OnEndCallback; set => _OnEndCallback=value; }

        protected abstract void Update();

        protected void End()
        {
            OnEnd();

            _OnEndCallback.Invoke();
            _OnEndCallback = null;
        }

        protected virtual void OnEnd()
        {

        }
    }
}
