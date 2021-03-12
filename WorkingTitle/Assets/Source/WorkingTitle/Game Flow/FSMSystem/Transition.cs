using Mirror;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

namespace WorkingTitle.GameFlow.FSMSystem
{
    public class Transition<T,U>
        where T : struct, IConvertible
        where U : struct, IConvertible
    {
        private readonly T _ID;

        private readonly U _SourceGameStateID;
        
        private readonly U _TargetGameStateID;

        private readonly NetworkedTransition _NetworkedTransitionPrefab;

        private NetworkedTransition _NetworkedTransitionInstance;

        private Action<Transition<T, U>> _OnEndCallback;

        public T ID => _ID;

        internal U SourceGameStateID => _SourceGameStateID;

        public U TargetGameStateID => _TargetGameStateID;

        internal Transition(T id, U sourceGameStateID, U targetGameStateID, NetworkedTransition networkedTransition)
        {
            _ID = id;
            _SourceGameStateID = sourceGameStateID;
            _TargetGameStateID = targetGameStateID;
            _NetworkedTransitionPrefab = networkedTransition;
        }

        internal void Begin(Action<Transition<T,U>> onEndCallback)
        {
            Debug.Log($"Began Transition: {ID}");

            _OnEndCallback = onEndCallback;

            _NetworkedTransitionInstance = UnityEngine.Object.Instantiate(_NetworkedTransitionPrefab);
            _NetworkedTransitionInstance.OnEndCallback = new Action(End);

            NetworkServer.Spawn(_NetworkedTransitionInstance.gameObject);
        }

        private void End()
        {
            Debug.Log($"Ended Transition: {ID}");

            NetworkServer.Destroy(_NetworkedTransitionInstance.gameObject);
            UnityEngine.Object.Destroy(_NetworkedTransitionInstance);

            _NetworkedTransitionInstance = null;

            _OnEndCallback.Invoke(this);
            _OnEndCallback = null;
        }
    }
}
