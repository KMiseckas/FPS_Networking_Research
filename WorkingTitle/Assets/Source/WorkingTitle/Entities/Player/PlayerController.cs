using System;
using UnityEngine;
using UnityEngine.InputSystem;
using WorkingTitle.Input;
using Mirror;
using System.Collections;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace WorkingTitle.Entities.Player
{
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerController : NetworkBehaviour
    {
        #region Fields

        /// <summary>
        /// The prefab of the player entity to spawn.
        /// </summary>
        [SerializeField] private GameObject _DefaultPlayerEntity;

        private PlayerEntity _PlayerEntity;

        private PlayerInput _input;
        private Vector2 _inputLook;

        private PlayerState _PlayerState;


        #endregion

        #region Properties

        public PlayerState PlayerState { set => _PlayerState = value; get => _PlayerState; }

        public PlayerEntity PlayerEntity { get => _PlayerEntity; set => _PlayerEntity=value; }

        #endregion

        #region Unity Messages

        public override void OnStartClient()
        {
            bool isOnHost = isServer && !isLocalPlayer;

            //Player controller should only exist on owner of the object.
            if((!isLocalPlayer && !isOnHost) || isOnHost)
            {
                return;
            }

            this.gameObject.SetActive(true);
        }

        public override void OnStartLocalPlayer()
        {
            HookInput();
            CmdSpawnPlayerEntity();
        }

        private void Update()
        {
            if(isLocalPlayer)
            {
                if (_PlayerEntity != null)
                {
                    Rotate (ConsumeRotation());
                    Move (GetAction("Player", "Move").ReadValue<Vector2>());
                }
            }
        }

        #endregion

        #region Functions

        private void HookInput()
        {
            BindAction("Player", "Look", this.Player_Look, ActionEventType.Performed);
            BindAction("Player", "Interact", this.Player_Interact, ActionEventType.Performed);
            BindAction("Player", "Jump", this.Player_Jump, ActionEventType.Performed);
            BindAction("Player", "Sprint", this.Player_BeginSprint, ActionEventType.Started);
            BindAction("Player", "Sprint", this.Player_EndSprint, ActionEventType.Canceled);
        }

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
            this._PlayerEntity.Rotate (new Vector2(rotationInput.y, rotationInput.x));
        }

        protected void Move(Vector2 movementInput)
        {
            _PlayerEntity.Move (new Vector3(movementInput.x, 0, movementInput.y));
        }

        /// <summary>
        /// Attach an instance of player entity to the player controller.
        /// </summary>
        /// <param name="playerEntity">Instance of the object to attach to this player controller</param>
        [Command]
        private void CmdSpawnPlayerEntity()
        {
            NetLog.ServerLog(this, $"Spawning player entity across all clients.");

            _PlayerEntity = Instantiate(_DefaultPlayerEntity).GetComponent<PlayerEntity>();
            _PlayerEntity.PlayerControllerID = this.netId;

            NetworkServer.Spawn(_PlayerEntity.gameObject, this.connectionToClient);
        }

        public void SetPlayerEntity(PlayerEntity playerEntity)
        {
            _PlayerEntity = playerEntity;
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
            this._PlayerEntity.IsSprinting = true;
        }

        /// <summary>
        ///     Handles the end sprint action event
        /// </summary>
        /// <param name="obj"></param>
        private void Player_EndSprint(InputAction.CallbackContext obj)
        {
            this._PlayerEntity.IsSprinting = false;
        }

        /// <summary>
        ///     Handles the player jump action event
        /// </summary>
        /// <param name="obj"></param>
        private void Player_Jump(InputAction.CallbackContext obj)
        {
            this._PlayerEntity.Jump();
        }

        /// <summary>
        ///     Handles the player interact action event
        /// </summary>
        /// <param name="obj"></param>
        private void Player_Interact(InputAction.CallbackContext obj)
        {
            this._PlayerEntity.Interact();
        }

        #endregion
    }

}
