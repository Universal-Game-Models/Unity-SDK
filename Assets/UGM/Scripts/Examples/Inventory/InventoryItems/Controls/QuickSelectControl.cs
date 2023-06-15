using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using static UGM.Core.UGMDataTypes;

namespace UGM.Examples.Inventory.InventoryItems.Controls
{
    /// <summary>
    /// Represents the QuickSelectControl class responsible for managing quick select buttons and their associated actions.
    /// </summary>
    public class QuickSelectControl : MonoBehaviour
    {
        private static QuickSelectControl _instance;

        /// <summary>
        /// Public property to access the instance of the QuickSelectControl script.
        /// </summary>
        public static QuickSelectControl Instance { get { return _instance; } }

        [Tooltip("Array of QuickSelect objects representing the quick select buttons and associated data.")]
        [SerializeField]
        private QuickSelect[] quickSelects;

        private void Awake()
        {
            // Check if an instance already exists
            if (_instance != null && _instance != this)
            {
                // Destroy the duplicate instance
                Destroy(this.gameObject);
            }
            else
            {
                // Set the instance if it doesn't exist
                _instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
        }

        /// <summary>
        /// Update function called once per frame. Checks if the pointer is not over a UI object,
        /// retrieves the number key pressed, and invokes the associated action for the corresponding quick select button.
        /// </summary>
        void Update()
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                var numberKeyPressed = GetNumberKeyPressed();
                if (numberKeyPressed >= 0 && quickSelects[numberKeyPressed].action != null)
                {
                    quickSelects[numberKeyPressed].action.Invoke();
                }
            }
        }

        /// <summary>
        /// Sets the quick select button with the specified number key.
        /// </summary>
        /// <param name="numberKeyPressed">The number key associated with the quick select button.</param>
        /// <param name="tokenInfo">The TokenInfo object containing the data for the quick select.</param>
        /// <param name="action">The UnityAction to be invoked when the quick select button is clicked.</param>
        public void SetQuickSelect(int numberKeyPressed, TokenInfo tokenInfo, UnityAction action, UnityAction secondaryAction)
        {
            if(quickSelects.Length <= numberKeyPressed)
            {
                Debug.LogError("Not enough quick selects assigned");
                return;
            }
            quickSelects[numberKeyPressed].Init(tokenInfo, action, secondaryAction);
        }

        /// <summary>
        /// Checks if a number key (0-9) is pressed and returns the corresponding number.
        /// </summary>
        /// <returns>The number key (0-9) that is pressed. Returns -1 if no number key is pressed.</returns>
        public int GetNumberKeyPressed()
        {
            for (int i = 0; i <= 9; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha0 + i) || Input.GetKeyDown(KeyCode.Keypad0 + i))
                {
                    return i;
                }
            }

            return -1; // Return -1 if no number key is pressed
        }
    }
}
