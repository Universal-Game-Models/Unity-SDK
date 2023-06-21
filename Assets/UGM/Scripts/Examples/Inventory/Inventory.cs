using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.EventSystems;
using UGM.Examples.Inventory.InventoryItems;
using static UGM.Core.UGMDataTypes;
using UGM.Examples.ThirdPersonController;

namespace UGM.Examples.Inventory
{
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
        [Tooltip("Reference to the GetNftsOwned component responsible for retrieving NFTs owned by a specific address.")]
        public GetNftsOwned nftsOwned;

        [Tooltip("The avatar loader that should be used by this inventory and its items")]
        public AvatarLoader avatarLoader;

        [Tooltip("Reference to the parent GameObject that contains the inventory UI elements.")]
        [SerializeField]
        protected GameObject parent;

        [Tooltip("Reference to the Transform component representing the content area where inventory items are displayed.")]
        [SerializeField]
        protected Transform content;

        [Tooltip("Reference to the prefab of the default inventory item.")]
        [SerializeField]
        protected InventoryItem defaultItemPrefab;

        [Tooltip("Array of item prefabs to be used for specific token attributes in the inventory.")]
        [SerializeField]
        protected ItemPrefabs[] itemPrefabs;

        [Tooltip("Structure defining the attributes of an item prefab used in the inventory.")]
        [Serializable]
        public struct ItemPrefabs
        {
            [Tooltip("The name of the attribute trait type or trait value")]
            public string name;
            [Tooltip("Flag to determine if the name is type or value")]
            public bool nameIsTraitType;
            [Tooltip("The inventory prefab that should be used when the name matches the attribute trait type or value")]
            public InventoryItem prefab;
        }

        /// <summary>
        /// List of TokenInfo objects representing the NFTs owned by a specific address.
        /// </summary>
        protected List<TokenInfo> tokenInfos;

        protected bool contentActive;

        /// <summary>
        /// Starts the execution of the script by loading data and updating the display of the inventory UI.
        /// </summary>
        public virtual async void Start()
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
        protected void OnDestroy()
        {
            ClearDisplay();
        }

        /// <summary>
        /// Updates the display of the inventory UI by creating and initializing inventory items based on token information.
        /// </summary>
        protected void UpdateDisplay()
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
                if (EventSystem.current.firstSelectedGameObject == null) EventSystem.current.firstSelectedGameObject = item.gameObject;
                item.Init(this, tokenInfo);
            }
        }

        /// <summary>
        /// Clears the display
        /// </summary>
        protected void ClearDisplay()
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
        public virtual void Update()
        {
            if (Input.GetKeyDown(KeyCode.I))
            {
                ToggleInventory();
            }
        }

        /// <summary>
        /// Toggles the visibility of the inventory UI by activating or deactivating the parent GameObject and invoking an event to show or hide the cursor.
        /// </summary>
        public virtual void ToggleInventory()
        {
            if (parent == null)
            {
                Debug.LogError("" + parent.name + " is missing!");
                return;
            }
            contentActive = !parent.activeInHierarchy;
            parent.SetActive(contentActive);
            ExampleUIEvents.OnShowCursor.Invoke(contentActive);
        }
    }
}