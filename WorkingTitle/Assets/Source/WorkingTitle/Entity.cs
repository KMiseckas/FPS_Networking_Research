using UnityEngine;

namespace WorkingTitle
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    public abstract class Entity : MonoBehaviour
    {
        #region Constants

        private const float MIN_ROT_X = -60f;
        private const float MAX_ROT_X = 60f;

        #endregion

        #region Properties

        public bool IsSprinting { get; protected set; }

        #endregion

        #region Fields

        private Rigidbody _rigidbody;
        private Camera _camera;
        [SerializeField] private float _walkSpeed;
        [SerializeField] private float _runSpeed;
        [SerializeField] private float _sensitivityX;
        [SerializeField] private float _sensitivityY;
        private Vector3 _inputMovement;
        private Vector2 _inputRotation;

        #endregion

        #region Unity Messages

        private void Awake()
        {
            this._rigidbody = GetComponent<Rigidbody>();
            this._camera = Camera.main;
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        protected virtual void Update()
        {
            this._camera.transform.localEulerAngles = new Vector3(-this._inputRotation.x, 0, 0);
            this.transform.localEulerAngles = new Vector3(0, this._inputRotation.y, 0);
        }

        private void FixedUpdate()
        {
            var newPosition = this._rigidbody.position;
            var speed = this.CalculateMovementSpeed();
            newPosition += this.transform.forward * this._inputMovement.z * speed * Time.deltaTime;
            newPosition += this.transform.right * this._inputMovement.x * speed * Time.deltaTime;

            this._rigidbody.MovePosition(newPosition);
        }

        #endregion

        #region Functions

        /// <summary>
        ///     Moves the player based on input
        /// </summary>
        /// <param name="inputMovement"></param>
        public void Move(Vector3 inputMovement)
        {
            this._inputMovement = inputMovement;
        }

        /// <summary>
        ///     Rotates the player based on input
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
            if (this.IsSprinting)
            {
                return this._runSpeed;
            }
            return this._walkSpeed;
        }

        #endregion
    }
}