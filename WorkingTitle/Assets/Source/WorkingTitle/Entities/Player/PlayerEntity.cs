using Mirror;
using UnityEngine;
using WorkingTitle.GameFlow;

namespace WorkingTitle.Entities.Player
{
    public class PlayerEntity : Entity
    {
        #region Constants

        private const float MIN_ROT_X = -60f;
        private const float MAX_ROT_X = 60f;

        #endregion

        #region Fields

        [SyncVar]
        private uint _PlayerControllerID;

        [SerializeField]
        private GameObject _Camera;

        #endregion

        #region

        public GameObject Camera{get => _Camera;}
        public uint PlayerControllerID { get => _PlayerControllerID; set => _PlayerControllerID=value; }

        #endregion

        #region Functions

        public override void OnStartClient()
        {
            base.OnStartClient();

            if(NetworkIdentity.spawned.TryGetValue(PlayerControllerID, out NetworkIdentity networkIdentity))
            {
                if(networkIdentity.isLocalPlayer)
                {
                    _Camera.SetActive(true);
                    this.gameObject.SetActive(true);

                    networkIdentity.GetComponent<PlayerController>().SetPlayerEntity(this);
                }
            }
        }

        public override void OnStartServer()
        {
            base.OnStartServer();

            this.enabled = true;

            _Camera.SetActive(false);
        }

        [Client]
        public void RotateCamera()
        {
            _InputRotation.x = Mathf.Clamp(_InputRotation.x, MIN_ROT_X, MAX_ROT_X);

            _Camera.transform.Rotate(Vector3.left, _InputRotation.x);
        }

        #endregion
    }
}