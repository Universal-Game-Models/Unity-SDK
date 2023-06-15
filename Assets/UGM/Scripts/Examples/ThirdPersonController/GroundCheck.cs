using UnityEngine;

namespace UGM.Examples.ThirdPersonController
{
    /// <summary>
    /// Checks if the object is grounded by performing a sphere cast downwards.
    /// </summary>
    public class GroundCheck : MonoBehaviour
    {
        [SerializeField][Tooltip("Useful for rough ground")]
        private float groundedOffset = -0.22f;
        [SerializeField][Tooltip("The radius of the grounded check")] 
        private float groundRadius = 0.28f;
        [SerializeField][Tooltip("Defines which layers to check for collisions (Should be different from player layer)")] 
        private LayerMask groundMask;

        /// <summary>
        /// Checks if the object is currently grounded.
        /// </summary>
        /// <returns>True if the object is grounded, false otherwise.</returns>
        public bool IsGrounded()
        {
            var position = transform.position;
            Vector3 spherePosition = new Vector3(position.x, position.y + groundedOffset,
                position.z);
            return Physics.CheckSphere(spherePosition, groundRadius, groundMask);
        }
    }
}
