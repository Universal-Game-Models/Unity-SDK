
using UnityEngine;

namespace UGM.Examples.ThirdPersonController
{
    /// <summary>
    /// Represents a camera orbit controller for player input.
    /// </summary>
    public class CameraOrbit : MonoBehaviour
    {
        private const float SMOOTH_TIME = 0.1f;
        
        [SerializeField][Tooltip("PlayerInput component is required to listen for input")]
        private CustomPlayerInput playerInput;
        [SerializeField][Tooltip("Used to set lower limit of camera rotation clamping")]
        private float minRotationX = -60f;
        [SerializeField][Tooltip("Used to set upper limit of camera rotation clamping")]
        private float maxRotationX = 50f;

        [SerializeField][Tooltip("Useful to apply smoothing to mouse input")]
        private bool smoothDamp = false;
        
        private Vector3 rotation;
        private Vector3 currentVelocity;

        private float pitch;
        private float yaw;

        private void Start()
        {
            rotation = transform.transform.eulerAngles;
            yaw = rotation.y;
            pitch = rotation.x;
        }

        /// <summary>
        /// Performs the camera rotation based on player input.
        /// </summary>
        private void LateUpdate()
        {
            if (playerInput == null) return;
            yaw += playerInput.MouseAxisX ;
            pitch -= playerInput.MouseAxisY ;

            if (smoothDamp)
            {
                rotation = Vector3.SmoothDamp(rotation, new Vector3(pitch, yaw), ref currentVelocity, SMOOTH_TIME);

            }
            else
            {
                rotation = new Vector3(pitch,yaw, rotation.z);
            }
            rotation.x = ClampAngle(rotation.x, minRotationX, maxRotationX);
            transform.transform.rotation = Quaternion.Euler(rotation);
        }

        /// <summary>
        /// Clamps the given angle between the minimum and maximum values.
        /// </summary>
        /// <param name="angle">The angle to clamp.</param>
        /// <param name="min">The minimum angle.</param>
        /// <param name="max">The maximum angle.</param>
        /// <returns>The clamped angle value.</returns>
        private float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360F)
            {
                angle += 360F;
            }
            if (angle > 360F)
            {
                angle -= 360F;
            }
            return Mathf.Clamp(angle, min, max);
        }
    }
}

