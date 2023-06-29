using System.Collections.Generic;
using UGM.Core;
using UnityEngine;
using static UGM.Core.UGMDataTypes;

namespace UGM.Examples.AnimationSelector
{
    /// <summary>
    /// Represents an animation selector that generates animation selector buttons based on metadata.
    /// </summary>
    public class AnimationSelector: MonoBehaviour
    {
        [Tooltip("The prefab for the animation selector button.")]
        [SerializeField]
        private AnimationSelectorButton animationSelectorButtonPrefab;
        [Tooltip("The parent GameObject to hide and show the animation selector buttons.")]
        [SerializeField]
        private GameObject parent;
        [Tooltip("The Transform component representing the content area for animation selector buttons.")]
        [SerializeField]
        private Transform content;
        [Tooltip("The UGMDownloader component responsible for metadata retrieval.")]
        [SerializeField]
        private UGMDownloader loader;
        [Tooltip("Determines if the animation should loop when played.")]
        [SerializeField]
        private bool loopAnimation = true;

        [Tooltip("Gameobject for button Animation List Button Example that shows or hides depending if there is an animation of the avatar")]
        [SerializeField]
        private GameObject animationButtonPanel;

        private bool contentActive = false;

        private void Awake()
        {
            if (!loader) loader = GetComponent<UGMDownloader>();
            if(!loader) loader = GetComponentInParent<UGMDownloader>();
            if(loader) loader.onMetadataSuccess.AddListener(OnMetadataSuccess);
        }

        private void OnDestroy()
        {
            if(loader) loader.onMetadataSuccess.RemoveListener(OnMetadataSuccess);
        }

        /// <summary>
        /// Event handler for successful metadata retrieval.
        /// </summary>
        /// <param name="metadata">The retrieved metadata.</param>
        private void OnMetadataSuccess(Metadata metadata)
        {
            List<string> animationNames = new List<string>();
            foreach (var attribute in metadata.attributes)
            {
                var attributeValue = attribute.value.ToString();
                if (attribute.trait_type == "Animation" && !animationNames.Contains(attributeValue))
                {
                    animationNames.Add(attributeValue);
                }
            }
            Init(animationNames.ToArray());
        }

        /// <summary>
        /// Initializes the animation selector with the specified animation names.
        /// </summary>
        /// <param name="animationNames">The array of animation names.</param>
        private void Init(string[] animationNames)
        {
            //Destroy the existing animation selector buttons
            for (int i = 0; i < content.childCount; i++)
            {
                Destroy(content.GetChild(i).gameObject);
            }
            //If there are no animations disable the content
            if (animationNames.Length == 0)
            {
                parent.gameObject.SetActive(false);
                ShowAnimationButtonPanel(false);
                return;
            }
            ShowAnimationButtonPanel(true);
            //Set the active to its current state and display all animation selector buttons
            parent.gameObject.SetActive(contentActive);
            foreach (var animationName in animationNames)
            {
                var newBtn = Instantiate(animationSelectorButtonPrefab, content);
                newBtn.Init(() =>
                {
                    if (loader.CurrentEmbeddedAnimationName == animationName)
                    {
                        loader.StopAnimation();
                    }
                    else
                    {
                        loader.PlayAnimation(animationName, loopAnimation);
                    }
                }, animationName);
            }
        }
        /// <summary>
        /// Determine if animation selector button should only show if the model has animations to be played.
        /// </summary>
        /// <param name="active"></param>
        private void ShowAnimationButtonPanel(bool active)
        {
            if (animationButtonPanel)
            {
                animationButtonPanel.SetActive(active);
            }
        }

        /// <summary>
        /// Update function called once per frame. Checks if the "B" key has been released and toggles the visibility of the content accordingly.
        /// </summary>
        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.B))
            {
                ToggleContent();
            }
        }

        /// <summary>
        /// Toggles the visibility of the content.
        /// </summary>
        /// <param name="active">The desired visibility state.</param>
        public void ToggleContent()
        {
            //Do not allow if their are no animations
            if (content.childCount <= 0) return;
            contentActive = !contentActive;
            parent.SetActive(contentActive);
            ExampleUIEvents.OnShowCursor.Invoke(contentActive);
        }

        /// <summary>
        /// Sets the UGMDownloader and metadata for the animation selector.
        /// </summary>
        /// <param name="uGMDownloader">The UGMDownloader to be set.</param>
        /// <param name="metadata">Optional metadata to be provided.</param>
        public void SetLoader(UGMDownloader uGMDownloader, Metadata metadata = null)
        {
            if(uGMDownloader == null)
            {
                Debug.LogWarning("The UGMDownloader was null");
                return;
            }
            if(loader != null)
            {
                loader.onMetadataSuccess.RemoveListener(OnMetadataSuccess);
            }
            loader = uGMDownloader;
            if (metadata != null)
            {
                OnMetadataSuccess(metadata);
            }

            //Wait for metadata to be downloaded
            //Must have UGMDownloader.loadMetadata = true before calling Load
            loader.onMetadataSuccess.AddListener(OnMetadataSuccess);      
        }
    }
}