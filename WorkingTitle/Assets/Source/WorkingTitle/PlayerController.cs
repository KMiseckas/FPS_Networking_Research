using System;
using UnityEngine;
using UnityEngine.InputSystem;
using WorkingTitle.Input;

namespace WorkingTitle
{
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerController : MonoBehaviour
    {
        #region Fields

        private PlayerInput _input;
        [SerializeField] private Player _pawn;
        private Vector2 _inputLook;

        #endregion

        #region Unity Messages

        private void OnEnable()
        {
            this.BindAction("Player", "Look", this.Player_Look, ActionEventType.Performed);
            this.BindAction("Player", "Interact", this.Player_Interact, ActionEventType.Performed);
            this.BindAction("Player", "Jump", this.Player_Jump, ActionEventType.Performed);
            this.BindAction("Player", "Sprint", this.Player_BeginSprint, ActionEventType.Started);
            this.BindAction("Player", "Sprint", this.Player_EndSprint, ActionEventType.Canceled);
        }

        private void Update()
        {
            var actionMove = this.GetAction("Player", "Move");
            var rotation = this.ConsumeRotation();
            this._pawn.Rotate(new Vector2()
            {
                x = rotation.y,
                y = rotation.x
            });

            var movement = actionMove.ReadValue<Vector2>();
            this._pawn.Move(new Vector3()
            {
                x = movement.x,
                y = 0,
                z = movement.y
            });
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
                this._input = this.GetComponent<PlayerInput>();

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
            this._pawn.IsSprinting = true;
        }

        /// <summary>
        ///     Handles the end sprint action event
        /// </summary>
        /// <param name="obj"></param>
        private void Player_EndSprint(InputAction.CallbackContext obj)
        {
            this._pawn.IsSprinting = false;
        }

        /// <summary>
        ///     Handles the player jump action event
        /// </summary>
        /// <param name="obj"></param>
        private void Player_Jump(InputAction.CallbackContext obj)
        {
            this._pawn.Jump();
        }

        /// <summary>
        ///     Handles the player interact action event
        /// </summary>
        /// <param name="obj"></param>
        private void Player_Interact(InputAction.CallbackContext obj)
        {
            this._pawn.Interact();
        }

        #endregion
    }
}
