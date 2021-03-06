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
        /// <summary>
        /// The time in seconds that determines how far in the past to interpolate the object for from the most up to date snapshot.
        /// </summary>
        protected const double INTERPOLATION_OFFSET = 0.1;

        /// <summary>
        /// Whether interpolation is enabled.
        /// </summary>
        [Header("Settings")]
        [SerializeField]
        private bool _InterpolationEnabled = true;

        /// <summary>
        /// Should rotation be synced?
        /// </summary>
        [Space]
        [SerializeField]
        private bool _SyncRotation = true;

        /// <summary>
        /// Should position be synced?
        /// </summary>
        [SerializeField]
        private bool _SyncPosition = true;

        /// <summary>
        /// The snapshot of the objects position as received from the server.
        /// </summary>
        [SyncVar(hook = nameof(OnPositionSynced))]
        private PositionSnapshot _PositionSnapshot = new PositionSnapshot();

        /// <summary>
        /// The snapshot of the objects rotation as received from the server
        /// </summary>
        [SyncVar(hook = nameof(OnRotationSynced))]
        private RotationSnapshot _RotationSnapshot = new RotationSnapshot();

        /// <summary>
        /// The current time of the intepolation for position.
        /// </summary>
        private double _ClientPositionInterpolationTime;

        /// <summary>
        /// The current time of the interpolation for rotation.
        /// </summary>
        private double _ClientRotationInterpolationTime;

        /// <summary>
        /// List of position snapshots as sent from the server. Culled once the oldest snapshot is no longer required.
        /// </summary>
        private List<PositionSnapshot> _PositionSnapshotList = new List<PositionSnapshot>();

        /// <summary>
        /// List of rotation snapshots as sent from the server. Culled once the oldest snapshot is no longer required.
        /// </summary>
        private List<RotationSnapshot> _RotationSnapshotList = new List<RotationSnapshot>();

        /// <summary>
        /// Current position of the object.
        /// </summary>
        private Vector3 _CurrentPosition = default;

        /// <summary>
        /// Current rotation of the object.
        /// </summary>
        private Vector3 _CurrentRotation = default;

        private void Awake()
        {
#if !UNITY_EDITOR
            _InterpolationEnabled = true;
#endif
        }

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

        /// <summary>
        /// Update call for when component is on client only object.
        /// </summary>
        protected virtual void UpdateClientOnly()
        {
            if(_SyncPosition) SetPosition(GetInterpolatedPosition());
            if(_SyncRotation) SetRotation(GetInterpolatedRotation());
        }

        /// <summary>
        /// Update call for when the component is on a server object.
        /// </summary>
        [Server]
        protected virtual void UpdateServerOnly()
        {
            if(_SyncPosition) RecordPositionData();
            if(_SyncRotation) RecordRotationData();
        }

        /// <summary>
        /// Get the smoothed position between two snapshots. This currently uses linear interpolation.
        /// </summary>
        /// <returns> A new Vector3 value for interpolated position</returns>
        [Client]
        private Vector3 GetInterpolatedPosition()
        {
#if UNITY_EDITOR
            if(!_InterpolationEnabled)
            {
                _PositionSnapshotList.Clear();

                return _CurrentPosition;
            }
#endif
            //If snapshots exist, increment time for the interpolation.
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

                        _PositionSnapshotList.RemoveRange(0, i);

                        break;
                    }
                }
            }

            return Vector3.Lerp(interpolateFromPosition, interpolateToPosition, interpolateFraction);
        }

        /// <summary>
        /// Get a smoothed rotation from between two snapshots.
        /// </summary>
        /// <returns> Rotation as euler angles</returns>
        [Client]
        private Vector3 GetInterpolatedRotation()
        {
#if UNITY_EDITOR
            if(!_InterpolationEnabled)
            {
                _RotationSnapshotList.Clear();

                return _CurrentRotation;
            }
#endif

            //If snapshots exist, increment time for the interpolation.
            if(_RotationSnapshotList.Count > 0)
            {
                _ClientRotationInterpolationTime += Time.unscaledDeltaTime;
            }

            Vector3 interpolateFromRotation = default;
            Vector3 interpolateToRotation = default;

            float interpolateFraction = 0;

            //Scan through every snapshot and find the two that fall around the current interpolation time.
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

                        _RotationSnapshotList.RemoveRange(0, i);

                        break;
                    }
                }
            }

            return Vector3.Lerp(interpolateFromRotation, interpolateToRotation, interpolateFraction);
        }

        /// <summary>
        /// Called by the server to record the position snapshot for this object.
        /// </summary>
        [Server]
        private void RecordPositionData()
        {
            _PositionSnapshot = new PositionSnapshot();

            _PositionSnapshot.PositionX = transform.position.x;
            _PositionSnapshot.PositionY = transform.position.y;
            _PositionSnapshot.PositionZ = transform.position.z;

            _PositionSnapshot.ServerTime = NetworkTime.time;
        }

        /// <summary>
        /// Called by the server to record the rotation snapshot for this object.
        /// </summary>
        [Server]
        private void RecordRotationData()
        {
            _RotationSnapshot = new RotationSnapshot();

            _RotationSnapshot.RotationX = transform.eulerAngles.x;
            _RotationSnapshot.RotationY = transform.eulerAngles.y;
            _RotationSnapshot.RotationZ = transform.eulerAngles.z;

            _RotationSnapshot.ServerTime = NetworkTime.time;
        }

        /// <summary>
        /// Called every time the position snapshot has been updated on the client.
        /// </summary>
        /// <param name="oldData">Value before the update.</param>
        /// <param name="newData">Value after the update.</param>
        [Client]
        protected virtual void OnPositionSynced(PositionSnapshot oldData, PositionSnapshot newData)
        {
            if(isClientOnly)
            {
#if UNITY_EDITOR
                if(_InterpolationEnabled)
                {
                    AddPositionSnapshotToCache(newData);
                }
                else
                {
                    _CurrentPosition = newData.ToVector3();
                }
#else
                AddPositionSnapshotToBuffer(newData);
#endif
            }
        }

        /// <summary>
        /// Called every time the rotation snapshot has been updated on the client.
        /// </summary>
        /// <param name="oldData">Value before the update.</param>
        /// <param name="newData">Value after the update.</param>
        [Client]
        protected virtual void OnRotationSynced(RotationSnapshot oldData, RotationSnapshot newData)
        {
            if(isClientOnly)
            {
#if UNITY_EDITOR
                if(_InterpolationEnabled)
                {
                    AddRotationSnapshotToCache(newData);
                }
                else
                {
                    _CurrentRotation = newData.ToVector3();
                }
#else
                AddRotationSnapshotToBuffer(newData);
#endif
            }
        }

        /// <summary>
        /// Add a position snapshot to the local cache of position snapshots.
        /// </summary>
        /// <param name="newPositionData"></param>
        [Client]
        protected void AddPositionSnapshotToCache(PositionSnapshot newPositionData)
        {
            //If this is the first snapshot in the cache, assign a interpolation time.
            if(_PositionSnapshotList.Count == 0)
            {
                _ClientPositionInterpolationTime = newPositionData.ServerTime - INTERPOLATION_OFFSET;
            }

            _PositionSnapshotList.Add(newPositionData);
        }

        /// <summary>
        /// Add a rotation snapshot the local cache of rotation snapshots.
        /// </summary>
        /// <param name="newRotationData"></param>
        [Client]
        protected void AddRotationSnapshotToCache(RotationSnapshot newRotationData)
        {
            //If this is the first snapshot in the cache, assign a interpolation time.
            if(_RotationSnapshotList.Count == 0)
            {
                _ClientRotationInterpolationTime = newRotationData.ServerTime - INTERPOLATION_OFFSET;
            }

            _RotationSnapshotList.Add(newRotationData);
        }

        /// <summary>
        /// Set the position of the object.
        /// </summary>
        /// <param name="position">Position which to apply to the object.</param>
        [Client]
        private void SetPosition(Vector3 position)
        {
            _CurrentPosition = position;

            transform.position = position;
        }

        /// <summary>
        /// Set the rotation of the object.
        /// </summary>
        /// <param name="position">Rotation which to apply to the object.</param>
        [Client]
        private void SetRotation(Vector3 rotation)
        {
            _CurrentRotation = rotation;

            transform.eulerAngles = rotation;
        }

        /// <summary>
        /// Struct for the data required to construct a position snapshot of the object.
        /// </summary>
        protected struct PositionSnapshot : IEquatable<PositionSnapshot>
        {
            public float PositionX;
            public float PositionY;
            public float PositionZ;

            /// <summary>
            /// The time at which the snapshot was recorded on the server.
            /// </summary>
            public double ServerTime;

            public bool Equals(PositionSnapshot other)
            {
                return !(other.PositionX != PositionX
                    || other.PositionY != PositionY
                    || other.PositionZ != PositionZ);
            }

            /// <summary>
            /// Create a Vector3 from the positions of x,y,z.
            /// </summary>
            /// <returns>New vector3 value</returns>
            public Vector3 ToVector3()
            {
                return new Vector3(PositionX, PositionY, PositionZ);
            }
        }

        /// <summary>
        /// Struct for the data required to construct a rotation snapshot of the object.
        /// </summary>
        protected struct RotationSnapshot : IEquatable<RotationSnapshot>
        {
            public float RotationX;
            public float RotationY;
            public float RotationZ;

            /// <summary>
            /// The time at which the snapshot was recorded on the server.
            /// </summary>
            public double ServerTime;

            public bool Equals(RotationSnapshot other)
            {
                return !(other.RotationX != RotationX
                    || other.RotationY != RotationY
                    || other.RotationZ != RotationZ);
            }

            /// <summary>
            /// Create a Vector3 from the rotations of x,y,z.
            /// </summary>
            /// <returns>New vector3 value</returns>
            public Vector3 ToVector3()
            {
                return new Vector3(RotationX, RotationY, RotationZ);
            }
        }
    }
}
