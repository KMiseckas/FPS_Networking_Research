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

        private PlayerState _PlayerState;

        private InputData _FrameInputData = new InputData();

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
        }

        public override void OnStartLocalPlayer()
        {
            this.gameObject.SetActive(true);

            HookInput();
            CmdSpawnPlayerEntity();
        }

        private void Update()
        {
            InputData cmdInputData = _FrameInputData;

            if(!cmdInputData.Equals(_FrameInputData))
            {
                NetLog.ClientLog(this, $"Moving Character : Forward ({_FrameInputData.MoveZ})");

                CmdApplyInputToPlayerEntity(_FrameInputData);
                RotateCamera(_FrameInputData.RotateAroundY);
            }
        }

        #endregion

        #region Functions

        private void HookInput()
        {
            BindAction("Player", "Look", Player_PerformLook, ActionEventType.Performed);
            BindAction("Player", "Look", Player_CancelLook, ActionEventType.Canceled);
            BindAction("Player", "Move", Player_PerformMove, ActionEventType.Performed);
            BindAction("Player", "Move", Player_CancelMove, ActionEventType.Canceled);
            BindAction("Player", "Interact", Player_PerformInteract, ActionEventType.Performed);
            BindAction("Player", "Interact", Player_CancelInteract, ActionEventType.Canceled);
            BindAction("Player", "Jump", Player_PerformJump, ActionEventType.Performed);
            BindAction("Player", "Jump", Player_CancelJump, ActionEventType.Canceled);
            BindAction("Player", "Sprint", Player_PerformSprint, ActionEventType.Performed);
            BindAction("Player", "Sprint", Player_CancelSprint, ActionEventType.Canceled);
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
        /// Apply input data from a <see cref="InputData"/> struct value.
        /// </summary>
        /// <param name="inputData"></param>
        [Command]
        protected void CmdApplyInputToPlayerEntity(InputData inputData)
        {
            NetLog.ServerLog(this,$"Moving Character : Forward ({inputData.MoveZ})");

            _PlayerEntity.IsSprinting = inputData.Sprinting;

            if(inputData.Jump)
            {
                _PlayerEntity.Jump();
            }

            _PlayerEntity.Move(new Vector3(inputData.MoveX, 0, inputData.MoveZ));
            _PlayerEntity.Rotate(new Vector2(inputData.RotateAroundY, inputData.RotateAroundX));
        }

        [Client]
        private void RotateCamera(float rotation)
        {
            if(_PlayerEntity != null)
            {
                _PlayerEntity.RotateCamera(rotation);
            }
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

            NetworkServer.Spawn(_PlayerEntity.gameObject);
        }

        /// <summary>
        /// Assign a player entity 
        /// </summary>
        /// <param name="playerEntity"></param>
        public void SetPlayerEntity(PlayerEntity playerEntity)
        {
            _PlayerEntity = playerEntity;

            this.enabled = true;
        }

        #endregion

        #region Events

        /// <summary>
        ///     Handles the player look action event
        /// </summary>
        /// <param name="obj"></param>
        private void Player_PerformLook(InputAction.CallbackContext obj)
        {
            Vector2 lookInput = obj.ReadValue<Vector2>();

            _FrameInputData.RotateAroundX = lookInput.x;
            _FrameInputData.RotateAroundY = lookInput.y;
        }

        /// <summary>
        ///     Handles the player look action event
        /// </summary>
        /// <param name="obj"></param>
        private void Player_CancelLook(InputAction.CallbackContext obj)
        {
            _FrameInputData.RotateAroundX = 0;
            _FrameInputData.RotateAroundY = 0;
        }

        private void Player_PerformMove(InputAction.CallbackContext obj)
        {
            Vector2 moveInput = obj.ReadValue<Vector2>();

            _FrameInputData.MoveX = moveInput.x;
            _FrameInputData.MoveZ = moveInput.y;
        }

        private void Player_CancelMove(InputAction.CallbackContext obj)
        {
            _FrameInputData.MoveX = 0;
            _FrameInputData.MoveZ = 0;
        }

        /// <summary>
        ///     Handles the begin sprint action event
        /// </summary>
        /// <param name="obj"></param>
        private void Player_PerformSprint(InputAction.CallbackContext obj)
        {
            _FrameInputData.Sprinting = true;
        }

        /// <summary>
        ///     Handles the end sprint action event
        /// </summary>
        /// <param name="obj"></param>
        private void Player_CancelSprint(InputAction.CallbackContext obj)
        {
            _FrameInputData.Sprinting = false;
        }

        /// <summary>
        ///     Handles the player start jump action event
        /// </summary>
        /// <param name="obj"></param>
        private void Player_PerformJump(InputAction.CallbackContext obj)
        {
            _FrameInputData.Jump = true;
        }

        /// <summary>
        ///     Handles the player end jump action event
        /// </summary>
        /// <param name="obj"></param>
        private void Player_CancelJump(InputAction.CallbackContext obj)
        {
            _FrameInputData.Jump = false;
        }

        /// <summary>
        ///     Handles the player interact action event
        /// </summary>
        /// <param name="obj"></param>
        private void Player_PerformInteract(InputAction.CallbackContext obj)
        {
            _FrameInputData.Interact = true;
        }

        /// <summary>
        ///     Handles the player interact action event
        /// </summary>
        /// <param name="obj"></param>
        private void Player_CancelInteract(InputAction.CallbackContext obj)
        {
            _FrameInputData.Interact = false;
        }

        /// <summary>
        /// Holds input data recorded by the player controller for a specific update frame.
        /// </summary>
        protected struct InputData : IEquatable<InputData>
        {
            public float MoveX;
            public float MoveZ;
            public bool Jump;
            public float RotateAroundY;
            public float RotateAroundX;
            public bool Interact;
            public bool Sprinting;

            public bool Equals(InputData other)
            {
                return other.MoveX != MoveX
                    || other.MoveZ != MoveZ
                    || other.Jump != Jump
                    || other.MoveX != MoveX
                    || other.MoveX != MoveX
                    || other.MoveX != MoveX
                    || other.MoveX != MoveX;
            }
        }

        #endregion
    }
}
