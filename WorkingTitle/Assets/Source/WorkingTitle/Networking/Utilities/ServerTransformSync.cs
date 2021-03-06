using Mirror;
using System;
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

        private double _ClientPositionInterpolationTime;

        private double _ClientRotationInterpolationTime;

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
            SetPosition(GetInterpolatedPosition());
            SetRotation(GetInterpolatedRotation());
        }

        [Server]
        protected virtual void UpdateServerOnly()
        {
            RecordPositionData();
            RecordRotationData();
        }

        [Client]
        private Vector3 GetInterpolatedPosition()
        {
            if(_PositionSnapshotList.Count > 0)
            {
                _ClientPositionInterpolationTime += Time.unscaledDeltaTime;
            }

            Vector3 interpolateFromPosition = default;
            Vector3 interpolateToPosition = default;

            float interpolateFraction = 0;

            for(int i = 0; i < _PositionSnapshotList.Count; i++)
            {
                if(i + 1 == _PositionSnapshotList.Count)
                {
                    if(_PositionSnapshotList[0].ServerTime > _ClientPositionInterpolationTime)
                    {
                        interpolateToPosition = _PositionSnapshotList[0].ToVector3();
                        interpolateFromPosition = interpolateToPosition;
                    }
                    else
                    {
                        interpolateToPosition = _PositionSnapshotList[i].ToVector3();
                        interpolateFromPosition = interpolateToPosition;
                    }

                    interpolateFraction = 0;
                }
                else
                {
                    PositionSnapshot fromSnapshot = _PositionSnapshotList[i];
                    PositionSnapshot toSnapshot = _PositionSnapshotList[i+1];

                    if(fromSnapshot.ServerTime <= _ClientPositionInterpolationTime && toSnapshot.ServerTime >= _ClientPositionInterpolationTime)
                    {
                        interpolateFromPosition = fromSnapshot.ToVector3();
                        interpolateToPosition = toSnapshot.ToVector3();

                        double snapshotDiff = toSnapshot.ServerTime - fromSnapshot.ServerTime;
                        double toTargetDiff = toSnapshot.ServerTime - _ClientPositionInterpolationTime;

                        interpolateFraction = 1 - Mathf.Clamp01((float)(toTargetDiff / snapshotDiff));

                        break;
                    }
                }
            }

            return Vector3.Lerp(interpolateFromPosition, interpolateToPosition, interpolateFraction);
        }

        [Client]
        private Vector3 GetInterpolatedRotation()
        {
            if(_RotationSnapshotList.Count > 0)
            {
                _ClientRotationInterpolationTime += Time.unscaledDeltaTime;
            }

            Vector3 interpolateFromRotation = default;
            Vector3 interpolateToRotation = default;

            float interpolateFraction = 0;

            for(int i = 0; i < _RotationSnapshotList.Count; i++)
            {
                if(i + 1 == _RotationSnapshotList.Count)
                {
                    if(_RotationSnapshotList[0].ServerTime > _ClientRotationInterpolationTime)
                    {
                        interpolateToRotation = _RotationSnapshotList[0].ToVector3();
                        interpolateFromRotation = interpolateToRotation;
                    }
                    else
                    {
                        interpolateToRotation = _RotationSnapshotList[i].ToVector3();
                        interpolateFromRotation = interpolateToRotation;
                    }

                    interpolateFraction = 0;
                }
                else
                {
                    RotationSnapshot fromSnapshot = _RotationSnapshotList[i];
                    RotationSnapshot toSnapshot = _RotationSnapshotList[i+1];

                    if(fromSnapshot.ServerTime <= _ClientRotationInterpolationTime && toSnapshot.ServerTime >= _ClientRotationInterpolationTime)
                    {
                        interpolateFromRotation = fromSnapshot.ToVector3();
                        interpolateToRotation = toSnapshot.ToVector3();

                        double snapshotDiff = toSnapshot.ServerTime - fromSnapshot.ServerTime;
                        double toTargetDiff = toSnapshot.ServerTime - _ClientRotationInterpolationTime;

                        interpolateFraction = 1 - Mathf.Clamp01((float)(toTargetDiff / snapshotDiff));

                        break;
                    }
                }
            }

            return Vector3.Lerp(interpolateFromRotation, interpolateToRotation, interpolateFraction);
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
                AddPositionSnapshotToBuffer(newData);
            }
        }

        [Client]
        protected virtual void OnRotationSynced(RotationSnapshot oldData, RotationSnapshot newData)
        {
            if(isClientOnly)
            {
                AddRotationSnapshotToBuffer(newData);
            }
        }

        [Client]
        protected void AddPositionSnapshotToBuffer(PositionSnapshot newPositionData)
        {
            if(_PositionSnapshotList.Count == 0)
            {
                _ClientPositionInterpolationTime = newPositionData.ServerTime - INTERPOLATION_OFFSET;
            }

            _PositionSnapshotList.Add(newPositionData);
        }

        [Client]
        protected void AddRotationSnapshotToBuffer(RotationSnapshot newRotationData)
        {
            if(_RotationSnapshotList.Count == 0)
            {
                _ClientRotationInterpolationTime = newRotationData.ServerTime - INTERPOLATION_OFFSET;
            }

            _RotationSnapshotList.Add(newRotationData);
        }

        [Client]
        private void SetPosition(Vector3 position)
        {
            transform.position = position;
        }

        [Client]
        private void SetRotation(Vector3 rotation)
        {
            transform.eulerAngles = rotation;
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

            public Vector3 ToVector3()
            {
                return new Vector3(PositionX, PositionY, PositionZ);
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

            public Vector3 ToVector3()
            {
                return new Vector3(RotationX, RotationY, RotationZ);
            }
        }
    }
}
