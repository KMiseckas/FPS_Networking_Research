using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorkingTitle.Networking.Utilities
{
    /// <summary>
    /// Syncs the game object transforms between server and client.
    /// </summary>
    public class ServerTransformSync : NetworkBehaviour
    {
        protected const double INTERPOLATION_OFFSET = 0.1;

        [Header("Settings")]
        [SerializeField]
        private bool _InterpolationEnabled = true;

        [Space]
        [SerializeField]
        private bool _SyncRotation = true;

        [SerializeField]
        private bool _SyncPosition = true;

        [SyncVar(hook = nameof(OnPositionSynced))]
        private PositionSnapshot _PositionSnapshot = new PositionSnapshot();

        [SyncVar(hook = nameof(OnRotationSynced))]
        private RotationSnapshot _RotationSnapshot = new RotationSnapshot();

        private double _TimeOfLastSnapshot;

        private double _ClientInterpolationTime;

        private List<PositionSnapshot> _PositionSnapshotList = new List<PositionSnapshot>();

        private List<RotationSnapshot> _RotationSnapshotList = new List<RotationSnapshot>();

        private void Update()
        {
            if(isClientOnly)
            {
                UpdateClientOnly();
            }
            else if(isServer)
            {
                UpdateServerOnly();
            }
        }

        protected virtual void UpdateClientOnly()
        {

        }

        [Server]
        protected virtual void UpdateServerOnly()
        {
            RecordPositionData();
            RecordRotationData();
        }

        [Server]
        private void RecordPositionData()
        {
            _PositionSnapshot = new PositionSnapshot();

            _PositionSnapshot.PositionX = transform.position.x;
            _PositionSnapshot.PositionY = transform.position.y;
            _PositionSnapshot.PositionZ = transform.position.z;

            _PositionSnapshot.ServerTime = NetworkTime.time;
        }

        [Server]
        private void RecordRotationData()
        {
            _RotationSnapshot = new RotationSnapshot();

            _RotationSnapshot.RotationX = transform.eulerAngles.x;
            _RotationSnapshot.RotationY = transform.eulerAngles.y;
            _RotationSnapshot.RotationZ = transform.eulerAngles.z;

            _RotationSnapshot.ServerTime = NetworkTime.time;
        }

        [Client]
        protected virtual void OnPositionSynced(PositionSnapshot oldData, PositionSnapshot newData)
        {
            if(isClientOnly)
            {
                ApplyPosition();
            }
        }

        [Client]
        protected virtual void OnRotationSynced(RotationSnapshot oldData, RotationSnapshot newData)
        {
            if(isClientOnly)
            {
                ApplyRotation();
            }
        }

        [Client]
        protected void ApplyPosition()
        {
            transform.position = new Vector3(_PositionSnapshot.PositionX, _PositionSnapshot.PositionY, _PositionSnapshot.PositionZ);
        }

        [Client]
        protected void ApplyRotation()
        {
            transform.eulerAngles = new Vector3(_RotationSnapshot.RotationX, _RotationSnapshot.RotationY, _RotationSnapshot.RotationZ);
        }

        protected struct PositionSnapshot : IEquatable<PositionSnapshot>
        {
            public float PositionX;
            public float PositionY;
            public float PositionZ;

            public double ServerTime;

            public bool Equals(PositionSnapshot other)
            {
                return !(other.PositionX != PositionX
                    || other.PositionY != PositionY
                    || other.PositionZ != PositionZ);
            }
        }

        protected struct RotationSnapshot : IEquatable<RotationSnapshot>
        {
            public float RotationX;
            public float RotationY;
            public float RotationZ;

            public double ServerTime;

            public bool Equals(RotationSnapshot other)
            {
                return !(other.RotationX != RotationX
                    || other.RotationY != RotationY
                    || other.RotationZ != RotationZ);
            }
        }
    }
}
