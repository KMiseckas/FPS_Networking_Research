using System;
using UnityEngine;
using UnityEngine.InputSystem;
using WorkingTitle.Input;
using Mirror;
using System.Collections;

namespace WorkingTitle.Entities.Player
{
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerController : NetworkBehaviour
    {
        #region Fields

        /// <summary>
        /// The prefab of the player entity to spawn.
        /// </summary>
        [SerializeField] private string _PlayerEntityToSpawnURL;

        private PlayerEntity _Pawn;

        [SyncVar(hook = nameof(OnPawnNetIDChanged))]
        private uint _PawnNetID;

        private PlayerInput _input;
        private Vector2 _inputLook;

        private PlayerState _playerState;


        #endregion

        #region Properties

        public PlayerState PlayerState
        {
            set => _playerState = value;
            get => _playerState;
        }

        public string PlayerEntityToSpawnURL
        {
            get => _PlayerEntityToSpawnURL;
        }

        #endregion

        #region Unity Messages

        private void Awake()
        {
            //Player controller should only exist on owner of the object.
            if(!isLocalPlayer || !isServer)
            {
                Destroy (this);
            }

            //Temp code to set url to null so we know server-client communication works as expected (in the future, the url would be null to start with).
            if(isServer)
            {
                _PlayerEntityToSpawnURL = null;
            }

            if(isLocalPlayer)
            {
                DeterminePlayerEntityURLToUse();
            }
        }

        private void OnEnable()
        {
            if(isLocalPlayer)
            {
                this.BindAction("Player", "Look", this.Player_Look, ActionEventType.Performed);
                this.BindAction("Player", "Interact", this.Player_Interact, ActionEventType.Performed);
                this.BindAction("Player", "Jump", this.Player_Jump, ActionEventType.Performed);
                this.BindAction("Player", "Sprint", this.Player_BeginSprint, ActionEventType.Started);
                this.BindAction("Player", "Sprint", this.Player_EndSprint, ActionEventType.Canceled);
            }
        }

        private void Update()
        {
            if(isLocalPlayer)
            {
                if (_Pawn != null)
                {
                    Rotate (ConsumeRotation());
                    Move (GetAction("Player", "Move").ReadValue<Vector2>());
                }
            }
        }

        #endregion

        #region Functions

        /// <summary>
        ///     Gets an action from the player input
        /// </summary>
        /// <param name="actionMapName"></param>
        /// <param name="actionName"></param>
        private InputAction GetAction(string actionMapName, string actionName)
        {
            if (this._input == null)
            {
                this._input = this.GetComponent<PlayerInput>();
            }

            var actionMap = this._input.actions.FindActionMap(actionMapName, true);
            return actionMap.FindAction(actionName, true);
        }

        /// <summary>
        ///     Binds an action to a player input action
        /// </summary>
        /// <param name="actionMapName"></param>
        /// <param name="actionName"></param>
        /// <param name="callback"></param>
        /// <param name="actionEventType"></param>
        private void BindAction(string actionMapName, string actionName, Action<InputAction.CallbackContext> callback, ActionEventType actionEventType)
        {
            var action = this.GetAction(actionMapName, actionName);

            switch (actionEventType)
            {
                case ActionEventType.Started:
                    action.started += callback;
                    break;
                case ActionEventType.Performed:
                    action.performed += callback;
                    break;
                case ActionEventType.Canceled:
                    action.canceled += callback;
                    break;
            }
        }

        /// <summary>
        ///     Consumes the rotation input
        /// </summary>
        private Vector2 ConsumeRotation()
        {
            var rotation = this._inputLook;
            this._inputLook = Vector2.zero;
            
            return rotation;
        }

        protected void Rotate(Vector2 rotationInput)
        {
            this._Pawn.Rotate (new Vector2(rotationInput.x, rotationInput.y));
        }

        protected void Move(Vector2 movementInput)
        {
            _Pawn.Move (new Vector3(movementInput.x, 0, movementInput.y));
        }

        protected void DeterminePlayerEntityURLToUse()
        {
            //TODO: Find out which player entity the player has chosen and set url here, for now we use a preset url.

            Cmd_SetPlayerEntityToSpawnURL(_PlayerEntityToSpawnURL);
        }

        [Command]
        private void Cmd_SetPlayerEntityToSpawnURL(string playerEntityURL)
        {
            _PlayerEntityToSpawnURL = playerEntityURL;
        }

        /// <summary>
        /// Attach an instance of player entity to the player controller.
        /// </summary>
        /// <param name="playerEntity">Instance of the object to attach to this player controller</param>
        public void AttachPlayerEntity(PlayerEntity playerEntity)
        {
            _PawnNetID = playerEntity.gameObject.GetComponent<NetworkIdentity>().netId;
            _Pawn = playerEntity;
        }

        private void OnPawnNetIDChanged(uint _, uint newValue)
        {
            if(NetworkIdentity.spawned.TryGetValue(_PawnNetID, out NetworkIdentity identity))
            {
                AttachPlayerEntity(identity.gameObject.GetComponent<PlayerEntity>());
            }
            else
            {
                StartCoroutine(SetTarget());
            }
        }

        IEnumerator SetTarget()
        {
            while(_Pawn == null)
            {
                yield return new WaitForSeconds(0.05f);

                if(NetworkIdentity.spawned.TryGetValue(_PawnNetID, out NetworkIdentity identity))
                {
                    AttachPlayerEntity(identity.gameObject.GetComponent<PlayerEntity>());
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        ///     Handles the player look action event
        /// </summary>
        /// <param name="obj"></param>
        private void Player_Look(InputAction.CallbackContext obj)
        {
            this._inputLook = obj.ReadValue<Vector2>();
        }

        /// <summary>
        ///     Handles the begin sprint action event
        /// </summary>
        /// <param name="obj"></param>
        private void Player_BeginSprint(InputAction.CallbackContext obj)
        {
            this._Pawn.IsSprinting = true;
        }

        /// <summary>
        ///     Handles the end sprint action event
        /// </summary>
        /// <param name="obj"></param>
        private void Player_EndSprint(InputAction.CallbackContext obj)
        {
            this._Pawn.IsSprinting = false;
        }

        /// <summary>
        ///     Handles the player jump action event
        /// </summary>
        /// <param name="obj"></param>
        private void Player_Jump(InputAction.CallbackContext obj)
        {
            this._Pawn.Jump();
        }

        /// <summary>
        ///     Handles the player interact action event
        /// </summary>
        /// <param name="obj"></param>
        private void Player_Interact(InputAction.CallbackContext obj)
        {
            this._Pawn.Interact();
        }

        #endregion
    }
}
