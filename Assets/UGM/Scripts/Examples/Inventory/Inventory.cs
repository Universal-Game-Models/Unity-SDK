using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static UGMManager;
using static UGMDataTypes;
using UnityEngine.EventSystems;

/// <summary>
/// The Inventory class manages the inventory system in the game.
/// It displays the inventory UI, loads and updates the inventory items based on the owned NFTs,
/// and allows toggling the visibility of the inventory UI.
/// </summary>
/// <remarks>
/// This class relies on the GetNftsOwned component to retrieve NFTs owned by a specific address.
/// It uses a parent GameObject to contain the inventory UI elements and a content Transform component
/// to display the inventory items. The class provides a default item prefab and an array of item prefabs
/// for specific token attributes. It creates and initializes inventory items based on the token information.
/// The inventory UI visibility can be toggled using the 'I' key, and an event is invoked to show or hide the cursor.
/// </remarks>
public class Inventory : MonoBehaviour
{
    /// <summary>
    /// Reference to the GetNftsOwned component responsible for retrieving NFTs owned by a specific address.
    /// </summary>
    public GetNftsOwned nftsOwned;

    /// <summary>
    /// Reference to the parent GameObject that contains the inventory UI elements.
    /// </summary>
    [SerializeField]
    private GameObject parent;
    /// <summary>
    /// Reference to the Transform component representing the content area where inventory items are displayed.
    /// </summary>
    [SerializeField]
    private Transform content;
    /// <summary>
    /// Reference to the prefab of the default inventory item.
    /// </summary>
    [SerializeField]
    private InventoryItem defaultItemPrefab;
    /// <summary>
    /// Array of item prefabs to be used for specific token attributes in the inventory.
    /// </summary>
    [SerializeField]
    private ItemPrefabs[] itemPrefabs;

    /// <summary>
    /// Structure defining the attributes of an item prefab used in the inventory.
    /// </summary>
    [Serializable]
    public struct ItemPrefabs
    {
        public string name;
        public bool nameIsTraitType;
        public InventoryItem prefab;
    }

    /// <summary>
    /// List of TokenInfo objects representing the NFTs owned by a specific address.
    /// </summary>
    private List<TokenInfo> tokenInfos;

    /// <summary>
    /// Starts the execution of the script by loading data and updating the display of the inventory UI.
    /// </summary>
    async void Start()
    {
        //Load the data
        tokenInfos = await nftsOwned.GetNftsByAddress();
        if (tokenInfos != null)
        {
            UpdateDisplay();
        }
    }

    /// <summary>
    /// Clears the display
    /// </summary>
    private void OnDestroy()
    {
        ClearDisplay();
    }

    /// <summary>
    /// Updates the display of the inventory UI by creating and initializing inventory items based on token information.
    /// </summary>
    private void UpdateDisplay()
    {
        ClearDisplay();
        foreach (var tokenInfo in tokenInfos)
        {
            InventoryItem prefab = null;
            foreach (var mi in itemPrefabs)
            {
                if (tokenInfo.metadata.attributes.FirstOrDefault(md => mi.nameIsTraitType ? md.trait_type == mi.name : md.value.ToString() == mi.name) != null)
                {
                    prefab = mi.prefab;
                    break;
                }
            }
            if (prefab == null)
            {
                Debug.Log("No match for item prefab, using default");
                prefab = defaultItemPrefab;
            }
            var item = Instantiate(prefab, content);
            if(EventSystem.current.firstSelectedGameObject == null) EventSystem.current.firstSelectedGameObject = item.gameObject;
            item.Init(tokenInfo);
        }
    }

    /// <summary>
    /// Clears the display
    /// </summary>
    private void ClearDisplay()
    {
        int childCount = content.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Destroy(content.GetChild(i).gameObject);
        }
    }

    /// <summary>
    /// Monitors input and toggles the visibility of the inventory UI when the 'I' key is pressed.
    /// </summary>
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventory();
        }
    }

    /// <summary>
    /// Toggles the visibility of the inventory UI by activating or deactivating the parent GameObject and invoking an event to show or hide the cursor.
    /// </summary>
    private void ToggleInventory()
    {
        var active = !parent.activeInHierarchy;
        parent.SetActive(active);
        ExampleUIEvents.OnShowCursor.Invoke(active);
    }
}
