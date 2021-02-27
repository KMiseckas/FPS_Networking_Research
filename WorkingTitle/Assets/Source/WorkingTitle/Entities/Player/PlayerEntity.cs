using Mirror;
using UnityEngine;
using WorkingTitle.GameFlow;

namespace WorkingTitle.Entities.Player
{
    public class PlayerEntity : Entity
    {
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
                    enabled = true;
                    _Camera.SetActive(true);

                    networkIdentity.GetComponent<PlayerController>().SetPlayerEntity(this);
                }
            }
        }

        public override void OnStartServer()
        {
            base.OnStartServer();

            enabled = false;
            _Camera.SetActive(false);
        }

        protected override void RotateImpl()
        {
            if(_Camera != null)
            {
                _Camera.transform.localEulerAngles = new Vector3(-_inputRotation.x, 0, 0);
            }

            base.RotateImpl();
        }

        #endregion
    }
}