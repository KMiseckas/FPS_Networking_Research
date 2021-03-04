using System;
using UnityEngine;
using UnityEngine.InputSystem;
using WorkingTitle.Input;
using Mirror;
using System.Collections.Generic;

namespace WorkingTitle.Entities.Player
{
    [RequireComponent(typeof(PlayerInput))]
    public partial class PlayerController : NetworkBehaviour
    {
        #region Fields

        /// <summary>
        /// The prefab of the player entity to spawn.
        /// </summary>
        [SerializeField] private GameObject _DefaultPlayerEntity;

        /// <summary>
        /// Entity that the controller sends recorded input to.
        /// </summary>
        [SerializeField] private PlayerEntity _PlayerEntity;

        /// <summary>
        /// Instance that provides input messages.
        /// </summary>
        private PlayerInput _Input;

        /// <summary>
        /// Reference to the instance of the player state object.
        /// </summary>
        private PlayerState _PlayerState;

        /// <summary>
        /// Reference to the input recorded by this object during the last input update frame.
        /// </summary>
        private InputData _FrameInputData = new InputData();

        private Dictionary<uint, InputData> _InputDataHistory = new Dictionary<uint, InputData>();

        private Dictionary<uint, InputResultData> _InputResultDataHistory = new Dictionary<uint, InputResultData>();

        private Queue<InputData> _InputDataQueue = new Queue<InputData>();

        private uint _CurrentInputDataID = 0;

        private uint _LastClearedID = 0;

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
            if((!isLocalPlayer && !isOnHost))
            {
                this.gameObject.SetActive(false);
            }
        }

        public override void OnStartLocalPlayer()
        {
            this.gameObject.SetActive(true);

            HookInput();
            CmdSpawnPlayerEntity();

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Confined;
        }

        public override void OnStartServer()
        {
            base.OnStartServer();

            this.gameObject.SetActive(true);
        }

        private void Update()
        {
            uint nextResultID = 0;
            bool hasDataToRecord = false;

            if(_PlayerEntity != null)
            {
                if(isLocalPlayer)
                {
                    hasDataToRecord = true;

                    nextResultID = _CurrentInputDataID++;

                    _FrameInputData.DeltaTime = Time.deltaTime;
                    _FrameInputData.ID = nextResultID;

                    ApplyInputToPlayerEntity(_FrameInputData);
                    CmdSendInputDataToPlayerEntity(_FrameInputData);
                    AddToInputDataHistory(_FrameInputData);

                    _PlayerEntity.UpdateEntity();
                }
                else if(isServer)
                {
                    hasDataToRecord = _InputDataQueue.Count > 0;

                    while(_InputDataQueue.Count > 0)
                    {
                        InputData nextInputData = _InputDataQueue.Dequeue();

                        ApplyInputToPlayerEntity(nextInputData);

                        nextResultID = nextInputData.ID;

                        _PlayerEntity.UpdateEntity();
                    }

                    _InputDataQueue.Clear();
                }

                if(hasDataToRecord)
                {
                    InputResultData resultData = RecordInputResults(nextResultID);

                    if(isLocalPlayer)
                    {
                        AddToInputResultDataHistory(resultData);
                    }
                    else if(isServer)
                    {
                        TargetVerifyClientInputResults(resultData);
                    }
                }
            }

            if(isLocalPlayer)
            {
                RotateCamera();
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
            if (this._Input == null)
            {
                this._Input = this.GetComponent<PlayerInput>();
            }

            var actionMap = this._Input.actions.FindActionMap(actionMapName, true);
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
        /// Apply input data from a <see cref="InputData"/> struct value to the player entity.
        /// </summary>
        /// <param name="inputData"></param>
        private void ApplyInputToPlayerEntity(InputData inputData)
        {
            _PlayerEntity.InputDeltaTime = inputData.DeltaTime;
            _PlayerEntity.IsSprinting = inputData.Sprinting;

            _PlayerEntity.AddMovementInput(new Vector3(inputData.MoveX, 0, inputData.MoveZ));
            _PlayerEntity.AddRotationInput(new Vector2(inputData.RotateAroundY, inputData.RotateAroundX));

            if(inputData.Jump)
            {
                _PlayerEntity.AddJumpInput();
            }

            if(inputData.Interact)
            {
                _PlayerEntity.AddInteractionInput();
            }
        }

        /// <summary>
        /// Send input data to the server to the player entity.
        /// </summary>
        /// <param name="inputData"></param>
        [Command]
        private void CmdSendInputDataToPlayerEntity(InputData inputData)
        {
            _InputDataQueue.Enqueue(inputData);        
        }

        [Client]
        private void AddToInputDataHistory(InputData inputData)
        {
            _InputDataHistory.Add(inputData.ID, inputData);
        }

        [Client]
        private void AddToInputResultDataHistory(InputResultData inputResultData)
        {
            _InputResultDataHistory.Add(inputResultData.ID, inputResultData);
        }

        private InputResultData RecordInputResults(uint resultID)
        {
            Vector3 position = _PlayerEntity.transform.position;
            float yRotation = _PlayerEntity.transform.rotation.eulerAngles.y;

            InputResultData resultData = new InputResultData();
            resultData.PositionX = position.x;
            resultData.PositionY = position.y;
            resultData.PositionZ = position.z;
            resultData.RotationY = yRotation;
            resultData.ID = resultID;

            return resultData;
        }

        [TargetRpc]
        private void TargetVerifyClientInputResults(InputResultData serverResultData)
        {
            bool foundResult = _InputResultDataHistory.TryGetValue(serverResultData.ID, out InputResultData clientResultData);

            if(foundResult)
            {
                if(!clientResultData.Equals(serverResultData))
                {
                    ApplyServerInputResults(serverResultData);
                    ReplayInputData(serverResultData.ID);

                    Debug.LogWarning("Out Of Sync - Re-Syncing client data");
                }

                ClearResultsUptoID(serverResultData.ID);
            }
            else
            {
                Debug.LogWarning($"Input Result Data [#{serverResultData.ID}] out of Sync! Something went wrong! The data stored in the client, with a specific `ID`, cannot be found.");
            }

        }

        [Client]
        private void ApplyServerInputResults(InputResultData inputResultData)
        {
            Transform transform = _PlayerEntity.transform;
            transform.position = new Vector3(inputResultData.PositionX, inputResultData.PositionY, inputResultData.PositionZ);
            transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.eulerAngles.x, inputResultData.RotationY, transform.rotation.eulerAngles.z));
        }

        [Client]
        private void ReplayInputData(uint startingID)
        {

        }

        [Client]
        private void ClearResultsUptoID(uint id)
        {
            for(uint i = _LastClearedID; i <= id; i++)
            {
                _InputDataHistory.Remove(i);
                _InputResultDataHistory.Remove(i);
            }

            _LastClearedID = id;
        }

        [Client]
        private void RotateCamera()
        {
            if(_PlayerEntity != null)
            {
                _PlayerEntity.RotateCamera();
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

        #endregion

        /// <summary>
        /// Holds input data recorded by the player controller for a single input frame.
        /// </summary>
        struct InputData : IEquatable<InputData>
        {
            public float MoveX;
            public float MoveZ;
            public bool Jump;
            public float RotateAroundY;
            public float RotateAroundX;
            public bool Interact;
            public bool Sprinting;

            public uint ID;

            public float DeltaTime;

            public bool Equals(InputData other)
            {
                return !(other.MoveX != MoveX
                    || other.MoveZ != MoveZ
                    || other.Jump != Jump
                    || other.RotateAroundY != RotateAroundY
                    || other.RotateAroundX != RotateAroundX
                    || other.Interact != Interact
                    || other.Sprinting != Sprinting
                    || other.DeltaTime != DeltaTime
                    || other.ID != ID);
            }
        }

        /// <summary>
        /// Holds the result data of inputs that have been applied to the <see cref="PlayerEntity"/>. Only hold results that are required for recoinciliation functionality.
        /// </summary>
        struct InputResultData : IEquatable<InputResultData>
        {
            public float PositionX;
            public float PositionY;
            public float PositionZ;

            public float RotationY;

            public uint ID;

            public bool Equals(InputResultData other)
            {
                return !(other.PositionX != PositionX
                    || other.PositionY != PositionY
                    || other.PositionZ != PositionZ
                    || other.RotationY != RotationY
                    || other.ID != ID);
            }
        }
    }
}
