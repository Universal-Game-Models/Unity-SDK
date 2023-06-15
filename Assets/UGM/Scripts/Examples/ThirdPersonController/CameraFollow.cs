using UnityEngine;

namespace UGM.Examples.ThirdPersonController
{
    /// <summary>
    /// Controls the camera to follow a target GameObject.
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        private const string TARGET_NOT_SET = "Target not set, disabling component";
        private readonly string TAG = typeof(CameraFollow).ToString();
        [SerializeField][Tooltip("The camera that will follow the target")]
        private Camera playerCamera;
        [SerializeField][Tooltip("The target Transform (GameObject) to follow")]
        private Transform target;
        [SerializeField][Tooltip("Defines the camera distance from the player along Z (forward) axis. Value should be negative to position behind the player")]
        private float cameraDistance = -2.4f;
        [SerializeField][Tooltip("Defines the camera offset from the player along X (right) axis. Value should be negative to position left of the player")]
        private float xOffset = 2f;
        [SerializeField][Tooltip("Defines the camera offset from the player along Y (up) axis. Value should be negative to position down of the player")]
        private float yOffset = 0;
        [SerializeField] private bool followOnStart = true;
        private bool isFollowing;
        
        private void Start()
        {
            if (target == null)
            {
                enabled = false;
                return;
            }

            if (followOnStart)
            {
                StartFollow();
            }
        }
        
        private void LateUpdate()
        {
            if (isFollowing)
            {
                playerCamera.transform.localPosition = (Vector3.forward * cameraDistance) + (Vector3.right * xOffset) + (Vector3.up * yOffset);
                playerCamera.transform.localRotation = Quaternion.Euler(Vector3.zero);
                transform.position = target.position;
            }
        }

        public void StopFollow()
        {
            isFollowing = false;
        }

        public void StartFollow()
        {
            isFollowing = true;
        }
    }
}
