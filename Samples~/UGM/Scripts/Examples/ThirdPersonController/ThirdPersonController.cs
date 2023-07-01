using UnityEngine;

namespace UGM.Examples.ThirdPersonController
{
    /// <summary>
    /// Handles the player input and controls the third-person character.
    /// Requires a ThirdPersonMovement and CustomPlayerInput component.
    /// </summary>
    [RequireComponent(typeof(ThirdPersonMovement),typeof(CustomPlayerInput))]
    public class ThirdPersonController : MonoBehaviour
    {
        private const float FALL_TIMEOUT = 0.15f;
            
        private static readonly int MoveSpeedHash = Animator.StringToHash("MoveSpeed");
        private static readonly int JumpHash = Animator.StringToHash("JumpTrigger");
        private static readonly int FreeFallHash = Animator.StringToHash("FreeFall");
        private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
        
        private Animator animator;
        private GameObject avatar;
        private ThirdPersonMovement thirdPersonMovement;
        private CustomPlayerInput playerInput;
        
        private float fallTimeoutDelta;
        
        [SerializeField][Tooltip("Useful to toggle input detection in editor")]
        private bool inputEnabled = true;
        private bool isInitialized;

        /// <summary>
        /// Initializes the ThirdPersonController by retrieving required components.
        /// </summary>
        private void Init()
        {
            thirdPersonMovement = GetComponent<ThirdPersonMovement>();
            playerInput = GetComponent<CustomPlayerInput>();
            playerInput.OnJumpPress += OnJump;
            isInitialized = true;
        }

        /// <summary>
        /// Sets up the ThirdPersonController for the target avatar.
        /// </summary>
        /// <param name="target">The target avatar GameObject.</param>
        public void Setup(GameObject target)
        {
            if (!isInitialized)
            {
                Init();
            }
            avatar = target;
            thirdPersonMovement.Setup(avatar);
            animator = avatar.GetComponent<Animator>();
        }

        /// <summary>
        /// Updates the controller, checking player input and updating the animator.
        /// </summary>
        private void Update()
        {
            if (avatar == null)
            {
                return;
            }
            if (inputEnabled)
            {
                playerInput.CheckInput();
                var xAxisInput = playerInput.AxisHorizontal;
                var yAxisInput = playerInput.AxisVertical;
                thirdPersonMovement.Move(xAxisInput, yAxisInput);
                thirdPersonMovement.SetIsRunning(playerInput.IsHoldingLeftShift);
            }
            UpdateAnimator();
        }

        /// <summary>
        /// Updates the animator parameters based on the character's movement state.
        /// </summary>
        private void UpdateAnimator()
        {
            var isGrounded = thirdPersonMovement.IsGrounded();
            animator.SetFloat(MoveSpeedHash, thirdPersonMovement.CurrentMoveSpeed);
            animator.SetBool(IsGroundedHash, isGrounded);
            if (isGrounded)
            {
                fallTimeoutDelta = FALL_TIMEOUT;
                animator.SetBool(FreeFallHash, false);
            }
            else
            {
                if (fallTimeoutDelta >= 0.0f)
                {
                    fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    animator.SetBool(FreeFallHash, true);
                }
            }
        }

        /// <summary>
        /// Handles the jump event and triggers the jump animation.
        /// </summary>
        private void OnJump()
        {
            if (thirdPersonMovement.TryJump())
            {
                animator.SetTrigger(JumpHash);
            }
        }
    }
}