using Mirror;
using UnityEngine;

namespace WorkingTitle.Entities
{
    [RequireComponent(typeof(Animator))]
    public abstract class Entity : NetworkBehaviour
    {
        #region Fields

        /// <summary>
        /// Component of `Character Controller` which handles player movement and collision logic.
        /// </summary>
        private CharacterController _CharacterControllerComponent;

        /// <summary>
        /// Component of `Animator` which handles the entity animations during movement, etc.
        /// </summary>
        private Animator _AnimatorComponent;

        [SerializeField] protected float _WalkSpeed;
        [SerializeField] protected float _RunSpeed;
        [SerializeField] protected float _SpeedMultiplierBackwards = 0.5f;
        [SerializeField] protected float _RotationSensitivityX;
        [SerializeField] protected float _RotationSensitivityY;
        [SerializeField] private float _JumpForce;
        [SerializeField] private float _Gravity = -15;

        /// <summary>
        /// Input for the X, Y and Z axis movement.
        /// </summary>
        protected Vector3 _InputMovement;

        /// <summary>
        /// Input for the entity rotation.
        /// </summary>
        protected Vector2 _InputRotation;

        private Vector3 _LastGroundedMovement;
        private float _LastGroundedSpeed;
        private Vector3 _LastGroundedDirection;
        private bool _LastIsGroundedCheck;

        private bool _IsSprinting;
        private bool _IsGrounded;
        private bool _IsJumping;

        private bool _JumpPressed;

        private float _InputDeltaTime;

        private Vector3 _GravityMovementVelocity;

        #endregion

        #region Properties

        public bool IsSprinting
        {
            get => _IsSprinting && _InputMovement.x == 0 && _InputMovement.z > 0;
            set => _IsSprinting = value;
        }

        public float InputDeltaTime { get => _InputDeltaTime; set => _InputDeltaTime=value; }

        #endregion

        #region Unity Messages

        protected virtual void Awake()
        {
            _CharacterControllerComponent = GetComponent<CharacterController>();
            _AnimatorComponent = GetComponent<Animator>();
        }

        #endregion

        #region Functions

        public virtual void UpdateEntity()
        {
            UpdateGroundedStatus();
            Rotate();
            Move();
        }

        /// <summary>
        ///     Applies input to later move the player
        /// </summary>
        /// <param name="inputMovement"></param>
        public virtual void AddMovementInput(Vector3 inputMovement)
        {
            if(_IsGrounded)
            {
                _LastGroundedMovement = _InputMovement;
            }
            
            _InputMovement = inputMovement;
        }

        /// <summary>
        ///     Applies input to later rotate the player
        /// </summary>
        /// <param name="inputRotation"></param>
        public virtual void AddRotationInput(Vector2 inputRotation)
        {
            _InputRotation.y = inputRotation.y * _RotationSensitivityY * InputDeltaTime;
            _InputRotation.x = inputRotation.x * _RotationSensitivityX * InputDeltaTime;
        }

        /// <summary>
        ///     Makes the player jump
        /// </summary>
        public virtual void AddJumpInput()
        {
            if(CanJump())
            {
                Jump();
            }
        }
        
        /// <summary>
        ///     Checks if the player can jump
        /// </summary>
        protected virtual bool CanJump()
        {
            return _IsGrounded && !_IsJumping;
        }

        /// <summary>
        ///     Player jump implementation
        /// </summary>
        protected virtual void Jump()
        {
            _IsJumping = true;
            _JumpPressed = true;
        }

        public virtual void AddInteractionInput()
        {

        }

        /// <summary>
        ///     Gets the movement input vector to use for movement
        /// </summary>
        private Vector3 GetMovementInput()
        {
            return _IsGrounded ? _InputMovement : _LastGroundedMovement;
        }

        /// <summary>
        ///     Calculates the players movement speed
        /// </summary>
        private float CalculateMovementSpeed()
        {
            if (!_IsGrounded)
            {
                return _LastGroundedSpeed;
            }

            Vector3 movement = GetMovementInput();
            float speed = _WalkSpeed;

            if (IsSprinting)
            {
                speed = _RunSpeed;
            }
            else if (movement.z < 0)
            {
                speed *= _SpeedMultiplierBackwards;
            }

            return speed;
        }

        /// <summary>
        ///     Calculates the players movement direction
        /// </summary>
        /// <param name="movementInput"></param>
        private Vector3 CalculateMovementDirection(Vector3 movementInput)
        {
            if(!_IsGrounded)
            {
                return _LastGroundedDirection;
            }

            Vector3 direction = this.transform.forward * movementInput.z + this.transform.right * movementInput.x;
            direction.Normalize();

            return direction;
        }

        /// <summary>
        ///     Movement implementation
        /// </summary>
        protected virtual void Move()
        {
            Vector3 movementInput = GetMovementInput();
            float speed = CalculateMovementSpeed();
            Vector3 direction = CalculateMovementDirection(movementInput);

            if (_IsGrounded && _GravityMovementVelocity.y < 0)
            {
                _GravityMovementVelocity.y = -1f;

                _LastGroundedSpeed = speed;
                _LastGroundedDirection = direction;
            }

            SetMovementAnimatorParams();

            Vector3 movementVelocity = direction * speed * InputDeltaTime;

            _CharacterControllerComponent.Move(movementVelocity);

            if(_JumpPressed && _IsGrounded)
            {
                _GravityMovementVelocity.y = Mathf.Sqrt(_JumpForce * -2f * _Gravity);
            }

            _GravityMovementVelocity.y += _Gravity * InputDeltaTime;

            _CharacterControllerComponent.Move(_GravityMovementVelocity * InputDeltaTime);

            _JumpPressed = false;
        }

        /// <summary>
        ///     Rotate implementation
        /// </summary>
        protected virtual void Rotate()
        {
            this.transform.Rotate(Vector3.up, _InputRotation.y);
        }

        /// <summary>
        ///     Sets the movement parameters of the animator 
        /// </summary>
        private void SetMovementAnimatorParams()
        {
            Vector2 animMovement = new Vector2()
            {
                x = _InputMovement.x,
                y = _InputMovement.z
            };

            if (IsSprinting)
            {
                animMovement.y = 2;
            }
            else if (_InputMovement.z < 0)
            {
                animMovement.y = -1;
                animMovement.x = -animMovement.x;
            }
                
            if (_IsGrounded && animMovement != Vector2.zero)
            {
                _AnimatorComponent.SetFloat("Horizontal", animMovement.x, 0.1f, InputDeltaTime);
                _AnimatorComponent.SetFloat("Vertical", animMovement.y, 0.1f, InputDeltaTime);
                _AnimatorComponent.SetFloat("WalkSpeed", IsSprinting ? 1f : 1.5f);
            }
            else
            {
                _AnimatorComponent.SetFloat("Horizontal", 0, 0.1f, InputDeltaTime);
                _AnimatorComponent.SetFloat("Vertical", 0, 0.1f, InputDeltaTime);
                _AnimatorComponent.SetFloat("WalkSpeed", 0);
            }
        }

        /// <summary>
        ///     Updates the is grounded status
        /// </summary>
        private void UpdateGroundedStatus()
        {
            bool isGrounded = _CharacterControllerComponent.isGrounded;

            if(_IsJumping)
            {
                if(_LastIsGroundedCheck && !isGrounded)
                {
                    _LastIsGroundedCheck = false;
                    _IsGrounded = false;
                    _IsJumping = false;
                }
            }
            else
            {
                _IsGrounded = isGrounded;
                _LastIsGroundedCheck = isGrounded;
            }
        }
        #endregion
    }
}