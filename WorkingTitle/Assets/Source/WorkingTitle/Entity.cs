using UnityEngine;

namespace WorkingTitle
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(Animator))]
    public abstract class Entity : MonoBehaviour
    {
        #region Constants

        private const float MIN_ROT_X = -60f;
        private const float MAX_ROT_X = 60f;

        #endregion

        #region Properties

        public bool IsSprinting
        {
            get => this._isSprinting && this._inputMovement.x == 0 && this._inputMovement.z > 0;
            set => this._isSprinting = value;
        }

        #endregion

        #region Fields

        [SerializeField] private bool _isPlayer;
        private Rigidbody _rigidbody;
        private Camera _camera;
        private Animator _animator;
        [SerializeField] private float _walkSpeed;
        [SerializeField] private float _runSpeed;
        [SerializeField] private float _sensitivityX;
        [SerializeField] private float _sensitivityY;
        private Vector3 _inputMovement;
        private Vector2 _inputRotation;
        private bool _isSprinting;
        [SerializeField] private float _groundDistanceCheck = 0.01f;
        private bool _isGrounded;

        #endregion

        #region Unity Messages

        protected virtual void Awake()
        {
            this._rigidbody = this.GetComponent<Rigidbody>();
            this._camera = Camera.main;
            this._animator = this.GetComponent<Animator>();
        }

        protected virtual void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        protected virtual void Update()
        {
            this.UpdateGroundedStatus();

            this.RotateImpl();

            if (this._isPlayer)
            {
                this.MoveImpl();
            }
        }

        #endregion

        #region Functions

        /// <summary>
        ///     Applies input to later move the player
        /// </summary>
        /// <param name="inputMovement"></param>
        public void Move(Vector3 inputMovement)
        {
            this._inputMovement = inputMovement;
        }

        /// <summary>
        ///     Applies input to later rotate the player
        /// </summary>
        /// <param name="inputRotation"></param>
        public void Rotate(Vector2 inputRotation)
        {
            this._inputRotation.x += inputRotation.x * this._sensitivityX * 10 * Time.deltaTime;
            this._inputRotation.x = Mathf.Clamp(this._inputRotation.x, MIN_ROT_X, MAX_ROT_X);
            this._inputRotation.y += inputRotation.y * this._sensitivityY * 10 * Time.deltaTime;
        }

        /// <summary>
        ///     Calculates the players movement speed
        /// </summary>
        private float CalculateMovementSpeed()
        {
            var speed = this._walkSpeed;

            if (this.IsSprinting)
                speed = this._runSpeed;
            else if (this._inputMovement.z < 0)
                speed *= .5f;

            return speed;
        }

        /// <summary>
        ///     Movement implementation
        /// </summary>
        private void MoveImpl()
        {
            var newPosition = this._rigidbody.position;
            var speed = this.CalculateMovementSpeed();
            var direction = this.transform.forward * this._inputMovement.z + this.transform.right * this._inputMovement.x;
            direction.Normalize();

            newPosition += direction * speed * Time.deltaTime;

            this.SetMovementAnimatorParams();

            this._rigidbody.MovePosition(newPosition);
        }

        /// <summary>
        ///     Rotate implementation
        /// </summary>
        private void RotateImpl()
        {
            this._camera.transform.localEulerAngles = new Vector3(-this._inputRotation.x, 0, 0);
            this.transform.localEulerAngles = new Vector3(0, this._inputRotation.y, 0);
        }

        /// <summary>
        ///     Sets the movement parameters of the animator 
        /// </summary>
        private void SetMovementAnimatorParams()
        {
            var animMovement = new Vector2()
            {
                x = _inputMovement.x,
                y = _inputMovement.z
            };

            if (this.IsSprinting)
                animMovement.y = 2;
            else if (this._inputMovement.z < 0)
            {
                animMovement.y = -1;
                animMovement.x = -animMovement.x;
            }
                
            if (this._isGrounded && animMovement != Vector2.zero)
            {
                this._animator.SetFloat("Horizontal", animMovement.x, 0.1f, Time.deltaTime);
                this._animator.SetFloat("Vertical", animMovement.y, 0.1f, Time.deltaTime);
                this._animator.SetFloat("WalkSpeed", this._isSprinting ? 1f : 1.5f);
            }
            else
            {
                this._animator.SetFloat("Horizontal", 0, 0.1f, Time.deltaTime);
                this._animator.SetFloat("Vertical", 0, 0.1f, Time.deltaTime);
                this._animator.SetFloat("WalkSpeed", 0);
            }
        }

        /// <summary>
        ///     Updates the is grounded status
        /// </summary>
        private void UpdateGroundedStatus()
        {
            this._isGrounded = Physics.CheckSphere(transform.position, this._groundDistanceCheck);
        }

        #endregion
    }
}