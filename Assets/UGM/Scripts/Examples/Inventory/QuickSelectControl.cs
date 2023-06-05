using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class QuickSelectControl : MonoBehaviour
{
    private static QuickSelectControl _instance;

    // Public property to access the instance
    public static QuickSelectControl Instance { get { return _instance; } }
    [Serializable]
    public class QuickSelect
    {
        public Button button;
        public Image image;
        public UGMAssetManager.ModelsOwnedTokenInfo tokenInfo;
        public UnityAction action;
    }

    public QuickSelect[] quickSelects;

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

    // Update is called once per frame
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

    public async void SetQuickSelect(int numberKeyPressed, UGMAssetManager.ModelsOwnedTokenInfo tokenInfo, UnityAction action)
    {
        var existing = Array.Find(quickSelects, q => q.tokenInfo == tokenInfo);
        if (existing != null)
        {
            existing.tokenInfo = null;
            existing.image.sprite = null;
            existing.button.onClick.RemoveAllListeners();
            existing.action = null;
        }
        //Set the new quick select
        quickSelects[numberKeyPressed].tokenInfo = tokenInfo;
        quickSelects[numberKeyPressed].action = action;
        quickSelects[numberKeyPressed].button.onClick.RemoveAllListeners();
        quickSelects[numberKeyPressed].button.onClick.AddListener(action);
        var texture = await UGMDownloader.DownloadImageAsync(tokenInfo.metadata.image);
        if (texture)
        {
            var image = quickSelects[numberKeyPressed].image;
            image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one / 2f);
            image.preserveAspect = true;
        }
    }
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
