using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UGMDataTypes;

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

    /// <summary>
    /// Represents a quick select button along with its associated data, including the button component, image component,
    /// token information, and the action to be performed when the button is clicked.
    /// </summary>
    [Serializable]
    public class QuickSelect
    {
        public Button button;
        public Image image;
        public TokenInfo tokenInfo;
        public UnityAction action;
    }

    /// <summary>
    /// Array of QuickSelect objects representing the quick select buttons and associated data.
    /// </summary>
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
    public async void SetQuickSelect(int numberKeyPressed, TokenInfo tokenInfo, UnityAction action)
    {
        var existing = Array.Find(quickSelects, q => q.tokenInfo == tokenInfo);
        if (existing != null)
        {
            existing.tokenInfo = null;
            if(existing.image) existing.image.sprite = null;
            if (existing.button) existing.button.onClick.RemoveAllListeners();     
            existing.action = null;
        }
        if(quickSelects.Length <= numberKeyPressed)
        {
            Debug.LogError("Not enough quick selects assigned");
            return;
        }
        //Set the new quick select
        quickSelects[numberKeyPressed].tokenInfo = tokenInfo;
        quickSelects[numberKeyPressed].action = action;
        if (quickSelects[numberKeyPressed].button)
        {
            quickSelects[numberKeyPressed].button.onClick.RemoveAllListeners();
            quickSelects[numberKeyPressed].button.onClick.AddListener(action);
        }
        var texture = await UGMDownloader.DownloadImageAsync(tokenInfo.metadata.image);
        if (texture)
        {
            var image = quickSelects[numberKeyPressed].image;
            if (image)
            {
                image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one / 2f);
                image.preserveAspect = true;
            }
        }
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
