using UnityEngine;

namespace UGM.Examples.ThirdPersonController
{
    /// <summary>
    /// Handles the movement and physics of a third-person character.
    /// Requires a CharacterController and GroundCheck component.
    /// </summary>
    [RequireComponent(typeof(CharacterController), typeof(GroundCheck))]
    public class ThirdPersonMovement : MonoBehaviour
    {
        private const float TURN_SMOOTH_TIME = 0.05f;

        [SerializeField][Tooltip("Used to determine movement direction based on input and camera forward axis")] 
        private Transform playerCamera;
        [SerializeField][Tooltip("Move speed of the character in")]
        private float walkSpeed = 3f;
        [SerializeField][Tooltip("Run speed of the character")] 
        private float runSpeed = 8f;
        [SerializeField][Tooltip("The character uses its own gravity value. The engine default is -9.81f")] 
        private float gravity = -18f;
        [SerializeField][Tooltip("The height the player can jump ")] 
        private float jumpHeight = 3f;
        [SerializeField]
        [Tooltip("Determines if the player faces same direction as playerCamera")]
        private bool matchCameraRotation;

        private CharacterController controller;
        private GameObject avatar;
        
        private float verticalVelocity;
        private float turnSmoothVelocity;

        private bool jumpTrigger;
        public float CurrentMoveSpeed { get; private set; }
        private bool isRunning;

        private GroundCheck groundCheck;

        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// Retrieves the CharacterController and GroundCheck components.
        /// </summary>
        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            groundCheck = GetComponent<GroundCheck>();
        }

        /// <summary>
        /// Sets up the third-person movement for the target avatar.
        /// </summary>
        /// <param name="target">The target avatar GameObject.</param>
        public void Setup(GameObject target)
        {
            avatar = target;
            if (playerCamera == null)
            {
                playerCamera = Camera.main.transform;
            }
        }

        /// <summary>
        /// Moves the character based on the input.
        /// </summary>
        /// <param name="inputX">The horizontal input axis.</param>
        /// <param name="inputY">The vertical input axis.</param>
        public void Move(float inputX, float inputY)
        {
            var moveDirection = playerCamera.right * inputX + playerCamera.forward * inputY;
            var moveSpeed = isRunning ? runSpeed: walkSpeed;

            JumpAndGravity();
            controller.Move(moveDirection.normalized * (moveSpeed * Time.deltaTime) +  new Vector3(0.0f, verticalVelocity * Time.deltaTime, 0.0f));

            var moveMagnitude = moveDirection.magnitude;
            CurrentMoveSpeed = isRunning ? runSpeed * moveMagnitude : walkSpeed * moveMagnitude;
            
            if (moveMagnitude > 0)
            {
                RotateAvatarTowardsMoveDirection(moveDirection);
            }
        }

        /// <summary>
        /// Rotates the avatar towards the move direction.
        /// </summary>
        /// <param name="moveDirection">The direction of movement.</param>
        private void RotateAvatarTowardsMoveDirection(Vector3 moveDirection)
        {
            if (matchCameraRotation && moveDirection.x == 0)
            {
                // Get the playerCamera's y rotation
                float cameraYRotation = playerCamera.transform.eulerAngles.y;

                // Set the avatar's rotation to match the playerCamera's y rotation
                avatar.transform.rotation = Quaternion.Euler(0, cameraYRotation, 0);
            }
            else
            {
                float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg + transform.rotation.y;
                float angle = Mathf.SmoothDampAngle(avatar.transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, TURN_SMOOTH_TIME);
                avatar.transform.rotation = Quaternion.Euler(0, angle, 0);
            }
        }

        /// <summary>
        /// Handles the jump and gravity logic.
        /// </summary>
        private void JumpAndGravity()
        {
            if (controller.isGrounded && verticalVelocity< 0)
            {
                verticalVelocity = -2f;
            }
            
            if (jumpTrigger && controller.isGrounded)
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                jumpTrigger = false;
            }
            
            verticalVelocity += gravity * Time.deltaTime;
        }

        /// <summary>
        /// Sets the running state of the character.
        /// </summary>
        /// <param name="running">True if the character is running, false otherwise.</param>
        public void SetIsRunning(bool running)
        {
            isRunning = running;
        }

        /// <summary>
        /// Attempts to trigger a jump action.
        /// </summary>
        /// <returns>True if the jump action is triggered, false otherwise.</returns>
        public bool TryJump()
        {
            jumpTrigger = false;
            if (controller.isGrounded)
            {
                jumpTrigger = true;
            }
            return jumpTrigger;
        }

        /// <summary>
        /// Checks if the character is grounded.
        /// </summary>
        /// <returns>True if the character is grounded, false otherwise.</returns>
        public bool IsGrounded()
        {
            if (verticalVelocity > 0)
            {
                return false;
            }
            return groundCheck.IsGrounded();
        }
    }
}
