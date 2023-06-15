using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UGM.Examples.AnimationSelector
{
    /// <summary>
    /// Represents a button for selecting an animation. 
    /// </summary>
    public class AnimationSelectorButton : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The button component used for selection.")]
        private Button selectorButton;

        [Tooltip("The text component displaying the animation name.")]
        [SerializeField]
        private TMPro.TextMeshProUGUI animationNameText;

        /// <summary>
        /// Initializes the AnimationSelectorButton with the specified button action and animation name.
        /// </summary>
        /// <param name="buttonAction">The UnityAction to be invoked when the button is clicked.</param>
        /// <param name="animationName">The name of the animation to be displayed.</param>
        public void Init(UnityAction buttonAction, string animationName)
        {
            selectorButton.onClick.RemoveAllListeners();
            selectorButton.onClick.AddListener(buttonAction);
            animationNameText.text = animationName;
        }
    }
}