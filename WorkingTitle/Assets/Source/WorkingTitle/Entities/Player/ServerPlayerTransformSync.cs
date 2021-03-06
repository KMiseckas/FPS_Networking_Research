using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WorkingTitle.Networking.Utilities;

namespace WorkingTitle.Entities.Player
{
    [RequireComponent(typeof(PlayerEntity))]
    public class ServerPlayerTransformSync : ServerTransformSync
    {
        [SerializeField]
        private bool _UpdateOwner = false;

        private bool _IsPartOfPossessedPlayerEntity;

        private bool _CanSync = true;

        public override void OnStartClient()
        {
            base.OnStartClient();

            _IsPartOfPossessedPlayerEntity = gameObject.GetComponent<PlayerEntity>().IsPossessed;

            if(_IsPartOfPossessedPlayerEntity)
            {
                _CanSync = _UpdateOwner;

                Debug.Log($"Can Sync = {_CanSync}");
            }
        }

        protected override void OnPositionSynced(PositionSnapshot oldData, PositionSnapshot newData)
        {
            Debug.Log("ON POSITION SYNCED - PARENT CLASS");

            if(_CanSync)
            {
                Debug.Log("ON POSITION SYNCED - PARENT CLASS - CAN SYNC!");

                base.OnPositionSynced(oldData, newData);
            }
        }

        protected override void OnRotationSynced(RotationSnapshot oldData, RotationSnapshot newData)
        {
            Debug.Log("ON ROTATION SYNCED - PARENT CLASS");

            if(_CanSync)
            {
                Debug.Log("ON ROTATION SYNCED - PARENT CLASS - CAN SYNC!");

                base.OnRotationSynced(oldData, newData);
            }
        }
    }
}
