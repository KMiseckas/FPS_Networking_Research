using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WorkingTitle.Networking.Utilities;

namespace WorkingTitle.Entities.Player
{
    /// <summary>
    /// An extended class of ServerTransformSync, this is specifically made for use with GameObject that are controlled by the player controller/possessed and do not require their transform updating.
    /// </summary>
    [RequireComponent(typeof(PlayerEntity))]
    public class ServerPlayerTransformSync : ServerTransformSync
    {
        /// <summary>
        /// Should the owner use interpolation. Set to false by default because player will usually have prediction enabled for predicting its movement on the client.
        /// </summary>
        [SerializeField]
        private bool _UpdateOwner = false;

        /// <summary>
        /// Is this object possessed by the player controller and can be considered as part of the local player.
        /// </summary>
        private bool _IsPartOfPossessedPlayerEntity;

        /// <summary>
        /// Can the transform be synced to this object.
        /// </summary>
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

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void UpdateClientOnly()
        {
            if(_CanSync)
            {
                base.UpdateClientOnly();
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void OnPositionSynced(PositionSnapshot oldData, PositionSnapshot newData)
        {
            if(_CanSync)
            {
                base.OnPositionSynced(oldData, newData);
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void OnRotationSynced(RotationSnapshot oldData, RotationSnapshot newData)
        {
            if(_CanSync)
            {
                base.OnRotationSynced(oldData, newData);
            }
        }
    }
}
