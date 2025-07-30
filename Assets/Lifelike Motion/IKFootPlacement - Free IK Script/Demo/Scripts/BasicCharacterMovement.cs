namespace LifelikeMotion.IKFootPlacement
{
    using UnityEngine;

    public class BasicCharacterMovement : MonoBehaviour
    {
        private CharacterController cc;
        [SerializeField] private IKFootPlacement iKFootPlacement;
        [SerializeField] private float movementSpeed = 2;
        [SerializeField] private float jumpSpeed = 5;
        [SerializeField] private float gravity = 15;
        [SerializeField] private float runSmoothSpeed = 5f;

        private bool receiveInput = true;
        private bool isMoving = true;
        private float horizontal;
        private float vertical;
        [HideInInspector] public bool jumped;

        private float runBoost = 0f;
        private bool isCrouching = false;
        private Vector3 velocity;
        private Vector3 ccPosition;
        private Animator animator;

        void Start()
        {
            cc = GetComponent<CharacterController>();
            animator = GetComponent<Animator>();

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        void Update()
        {
            GetInputData();

            // Handle crouch input
            isCrouching = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            animator.SetBool("Crouched", isCrouching);

            // Handle run boost (but clamp if crouching)
            bool isShiftHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            float targetBoost = (isShiftHeld && !isCrouching) ? 1f : 0f;
            runBoost = Mathf.MoveTowards(runBoost, targetBoost, Time.deltaTime * runSmoothSpeed);

            CalculateMovement();
        }

        public void CalculateMovement()
        {
            Vector3 _velocity = Vector3.zero;
            _velocity.z = vertical;
            _velocity.x = horizontal;

            float totalMultiplier = 1f + runBoost;
            _velocity *= totalMultiplier;
            _velocity = Vector3.ClampMagnitude(_velocity, 2f);

            // Set movement floats
            animator.SetFloat("Z", _velocity.z);
            animator.SetFloat("X", _velocity.x);
            animator.SetFloat("Move_Y", _velocity.z);
            animator.SetFloat("Move_X", _velocity.x);

            velocity.z = _velocity.z * movementSpeed;
            velocity.x = _velocity.x * movementSpeed;

            if (cc.isGrounded && !jumped)
            {
                velocity.y = -2;
            }
            else if (cc.isGrounded && jumped)
            {
                velocity.y = jumpSpeed;

                if (iKFootPlacement != null)
                {
                    iKFootPlacement.isGrounded = false;
                    iKFootPlacement.jumped = true;
                }

                isMoving = true;
                jumped = false;
            }
            else
            {
                velocity.y -= gravity * Time.deltaTime;
                isMoving = true;
            }

            cc.Move(transform.TransformVector(velocity) * Time.deltaTime);

            if (!isMoving)
            {
                cc.transform.position = new Vector3(ccPosition.x, cc.transform.position.y, ccPosition.z);
            }
            else
            {
                ccPosition = cc.transform.position;
            }
        }

        private void GetInputData()
        {
            if (receiveInput)
            {
                vertical = Input.GetAxis("Vertical");
                horizontal = Input.GetAxis("Horizontal");

                if (iKFootPlacement != null)
                {
                    iKFootPlacement.isGrounded = cc.isGrounded;
                }

                if (vertical != 0 || horizontal != 0)
                {
                    isMoving = true;
                    if (iKFootPlacement != null) iKFootPlacement.isMoving = true;
                }
                else
                {
                    isMoving = false;
                    if (iKFootPlacement != null) iKFootPlacement.isMoving = false;
                }

                if (Input.GetButtonDown("Jump") && cc.isGrounded && !jumped)
                {
                    jumped = true;
                }
                else
                {
                    jumped = false;
                }
            }
        }
    }
}
