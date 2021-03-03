using Mirror;
using UnityEngine;

namespace WorkingTitle.Entities
{
    [RequireComponent(typeof(Animator))]
    public abstract class Entity : NetworkBehaviour
    {
        #region Fields

        private Rigidbody _rigidbody;
        private CharacterController _characterController;
        private Animator _animator;
        [SerializeField] protected float _walkSpeed;
        [SerializeField] protected float _runSpeed;
        [SerializeField] protected float _sensitivityX;
        [SerializeField] protected float _SensitivityY;
        protected Vector3 _InputMovement;
        protected Vector2 _InputRotation;

        [SerializeField] private float _groundDistanceCheck = 0.01f; //For physics controller
        private Vector3 _LastGroundedMovement;
        private float _lastGroundedSpeed;
        private Vector3 _lastGroundedDirection;
        private bool _lastIsGroundedCheck;
        [SerializeField] private float _jumpForce;
        private Vector3 _velocity;
        private Vector3 _jumpVelocity;

        private bool _isSprinting;
        private bool _IsGrounded;
        private bool _isJumping;

        internal float _InputDeltaTime;

        #endregion

        #region Properties

        public bool IsSprinting
        {
            get => this._isSprinting && this._InputMovement.x == 0 && this._InputMovement.z > 0;
            set => this._isSprinting = value;
        }
        public float InputDeltaTime { get => _InputDeltaTime; set => _InputDeltaTime=value; }

        #endregion

        #region Unity Messages

        protected virtual void Awake()
        {
            this._rigidbody = this.GetComponent<Rigidbody>();
            this._characterController = this.GetComponent<CharacterController>();
            this._animator = this.GetComponent<Animator>();
        }

        protected virtual void Update()
        {
            UpdateGroundedStatus();
            RotateImpl();
            MoveImpl();
        }

        #endregion

        #region Functions

        /// <summary>
        ///     Applies input to later move the player
        /// </summary>
        /// <param name="inputMovement"></param>
        public virtual void Move(Vector3 inputMovement)
        {
            Debug.Assert(isLocalPlayer);

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
        public virtual void Rotate(Vector2 inputRotation)
        {
            _InputRotation.y += inputRotation.y * this._SensitivityY * 10 * InputDeltaTime;
        }

        /// <summary>
        ///     Makes the player jump
        /// </summary>
        public virtual void Jump()
        {
            if(this.CanJump())
            {
                this.JumpImpl();
            }
        }

        public virtual void Interact()
        {

        }
        
        /// <summary>
        ///     Checks if the player can jump
        /// </summary>
        protected virtual bool CanJump() => this._IsGrounded && !this._isJumping;

        /// <summary>
        ///     Player jump implementation
        /// </summary>
        protected virtual void JumpImpl()
        {
            this._isJumping = true;
            this._IsGrounded = false;
            this._jumpVelocity = Vector3.up * this._jumpForce * 2;
        }

        /// <summary>
        ///     Gets the movement input vector to use for movement
        /// </summary>
        private Vector3 GetMovementInput() => this._IsGrounded ? this._InputMovement : this._LastGroundedMovement;

        /// <summary>
        ///     Calculates the players movement speed
        /// </summary>
        private float CalculateMovementSpeed()
        {
            if (!this._IsGrounded)
                return this._lastGroundedSpeed;

            var movement = this.GetMovementInput();
            var speed = this._walkSpeed;

            if (this.IsSprinting)
                speed = this._runSpeed;
            else if (movement.z < 0)
                speed *= .5f;

            return speed;
        }

        /// <summary>
        ///     Calculates the players movement direction
        /// </summary>
        /// <param name="movement"></param>
        private Vector3 CalculateMovementDirection(Vector3 movement)
        {
            if (!this._IsGrounded)
                return this._lastGroundedDirection;

            var direction = this.transform.forward * movement.z + this.transform.right * movement.x;
            direction.Normalize();
            return direction;
        }

        /// <summary>
        ///     Movement implementation
        /// </summary>
        protected virtual void MoveImpl()
        {
            var movement = this.GetMovementInput();
            var speed = this.CalculateMovementSpeed();
            var direction = this.CalculateMovementDirection(movement);

            if (this._IsGrounded)
            {
                this._lastGroundedSpeed = speed;
                this._lastGroundedDirection = direction;
            }

            this.SetMovementAnimatorParams();

            var velocity = direction * speed * InputDeltaTime;
            this._velocity.x = velocity.x;
            this._velocity.z = velocity.z;

            // Changes the height position of the player..
            if (this._jumpVelocity != Vector3.zero)
            {
                this._velocity.y = 0;
                this._velocity += this._jumpVelocity;
                this._jumpVelocity = Vector3.zero;
            }

            if(!this._IsGrounded)
            {
                this._velocity.y += Physics.gravity.y * InputDeltaTime;
            }
            
            this._characterController.Move(this._velocity);
        }

        /// <summary>
        ///     Rotate implementation
        /// </summary>
        protected virtual void RotateImpl()
        {
            transform.localEulerAngles = new Vector3(0, _InputRotation.y, 0);
        }

        /// <summary>
        ///     Sets the movement parameters of the animator 
        /// </summary>
        private void SetMovementAnimatorParams()
        {
            var animMovement = new Vector2()
            {
                x = _InputMovement.x,
                y = _InputMovement.z
            };

            if (this.IsSprinting)
                animMovement.y = 2;
            else if (this._InputMovement.z < 0)
            {
                animMovement.y = -1;
                animMovement.x = -animMovement.x;
            }
                
            if (this._IsGrounded && animMovement != Vector2.zero)
            {
                this._animator.SetFloat("Horizontal", animMovement.x, 0.1f, InputDeltaTime);
                this._animator.SetFloat("Vertical", animMovement.y, 0.1f, InputDeltaTime);
                this._animator.SetFloat("WalkSpeed", this._isSprinting ? 1f : 1.5f);
            }
            else
            {
                this._animator.SetFloat("Horizontal", 0, 0.1f, InputDeltaTime);
                this._animator.SetFloat("Vertical", 0, 0.1f, InputDeltaTime);
                this._animator.SetFloat("WalkSpeed", 0);
            }
        }

        /// <summary>
        ///     Updates the is grounded status
        /// </summary>
        private void UpdateGroundedStatus()
        {
#if UNITY_PLAYER_PHYSICS
            var isGrounded = Physics.Raycast(transform.position + Vector3.up * this._groundDistanceCheck * .5f, Vector3.down, this._groundDistanceCheck);
#else
            var isGrounded = this._characterController.isGrounded;
#endif
            if(this._isJumping)
            {
                if(this._lastIsGroundedCheck && !isGrounded)
                {
                    this._lastIsGroundedCheck = false;
                    this._IsGrounded = false;
                    this._isJumping = false;
                }
            }
            else
            {
                this._IsGrounded = isGrounded;
                this._lastIsGroundedCheck = isGrounded;
            }
        }

#endregion
    }
}