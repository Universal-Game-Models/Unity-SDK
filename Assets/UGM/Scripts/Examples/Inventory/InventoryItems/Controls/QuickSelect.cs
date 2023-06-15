using UGM.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UGM.Core.UGMDataTypes;

namespace UGM.Examples.Inventory.InventoryItems.Controls
{
    /// <summary>
    /// Represents a quick select button along with its associated data, including the button component, image component,
    /// token information, and the action to be performed when the button is clicked.
    /// </summary>
    public class QuickSelect : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField]
        [Tooltip("The button component associated with the quick select.")]
        private Button button;

        [SerializeField]
        [Tooltip("The image component associated with the quick select.")]
        private Image image;

        [Tooltip("The action to be performed when the quick select button is clicked.")]
        public UnityAction action;
        
        [Tooltip("The action to be performed when the quick select secondary button is clicked.")]
        public UnityAction secondaryAction;

        public async void Init(TokenInfo tokenInfo, UnityAction action, UnityAction secondaryAction)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
            this.action = action;
            this.secondaryAction = secondaryAction;
            var texture = await UGMDownloader.DownloadImageAsync(tokenInfo.metadata.image);
            if (texture && image)
            {
                image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one / 2f);
                image.preserveAspect = true;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                secondaryAction.Invoke();
            }
        }
    }
}

