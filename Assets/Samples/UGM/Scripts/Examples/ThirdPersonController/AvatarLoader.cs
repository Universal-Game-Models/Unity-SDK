using UGM.Core;
using UnityEngine;

namespace UGM.Examples.ThirdPersonController
{
    /// <summary>
    /// Manages the loading and setup of avatars, including animators and controllers.
    /// Inherits from the UGMDownloader class.
    /// </summary>
    public class AvatarLoader : UGMDownloader
    {
        [SerializeField]
        [Tooltip("Preview avatar to display until avatar loads. Will be destroyed after new avatar is loaded")]
        private GameObject previewCharacter;

        private Animator animator;

        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// Retrieves the Animator component attached to the GameObject.
        /// </summary>
        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        /// <summary>
        /// Called before the first frame update.
        /// Calls the base Start method from the parent class.
        /// Sets up the preview avatar if available.
        /// </summary>
        protected override void Start()
        {
            base.Start();
            if (previewCharacter != null)
            {
                SetupAvatar(previewCharacter);
            }
        }

        /// <summary>
        /// Called when the model loading succeeds.
        /// Destroys the preview character, sets up the target avatar, and calls the base OnModelSuccess method from the parent class.
        /// </summary>
        protected override void OnModelSuccess(GameObject targetAvatar)
        {
            if (previewCharacter != null)
            {
                Destroy(previewCharacter);
                previewCharacter = null;
            }
            SetupAvatar(targetAvatar);
            base.OnModelSuccess(targetAvatar);
        }

        /// <summary>
        /// Called when the animation starts playing.
        /// Disables the animator to allow the Animation component to play.
        /// Calls the base OnAnimationStart method from the parent class.
        /// </summary>
        protected override void OnAnimationStart(string animationName)
        {
            animator.enabled = false;
            base.OnAnimationStart(animationName);
        }

        /// <summary>
        /// Called when the animation completes.
        /// Calls the base OnAnimationEnd method from the parent class.
        /// Enables the animator.
        /// </summary>
        protected override void OnAnimationEnd(string animationName)
        {
            base.OnAnimationEnd(animationName);
            animator.enabled = true;
        }

        /// <summary>
        /// Sets up the target avatar by configuring the animator and third-person controller.
        /// </summary>
        private void SetupAvatar(GameObject targetAvatar)
        {
            SetupAnimator();

            var controller = GetComponent<global::UGM.Examples.ThirdPersonController.ThirdPersonController>();
            if (controller != null)
            {
                controller.Setup(gameObject);
            }
        }

        /// <summary>
        /// Sets up the animator by removing the old animator and adding a new one with the updated settings.
        /// </summary>
        private void SetupAnimator()
        {
            animator.avatar = AvatarCreator.CreateAvatar(InstantiatedGO);
        }
    }
}
