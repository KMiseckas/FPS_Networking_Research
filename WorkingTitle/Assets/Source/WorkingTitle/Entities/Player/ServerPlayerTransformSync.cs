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
            }
        }

        protected override void UpdateClientOnly()
        {
            if(_CanSync)
            {
                base.UpdateClientOnly();
            }
        }

        protected override void OnPositionSynced(PositionSnapshot oldData, PositionSnapshot newData)
        {
            if(_CanSync)
            {
                base.OnPositionSynced(oldData, newData);
            }
        }

        protected override void OnRotationSynced(RotationSnapshot oldData, RotationSnapshot newData)
        {
            if(_CanSync)
            {
                base.OnRotationSynced(oldData, newData);
            }
        }
    }
}
