using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static UGMManager;
using static UGMDataTypes;
using UnityEngine.EventSystems;

public class Inventory : MonoBehaviour
{
    public GetNftsOwned nftsOwned;

    [SerializeField]
    private GameObject parent;
    [SerializeField]
    private Transform content;
    [SerializeField]
    private InventoryItem defaultItemPrefab;
    [SerializeField]
    private ItemPrefabs[] itemPrefabs;

    [Serializable]
    public struct ItemPrefabs
    {
        public string name;
        public bool nameIsTraitType;
        public InventoryItem prefab;
    }

    private List<TokenInfo> tokenInfos;

    // Start is called before the first frame update
    async void Start()
    {
        //Load the data
        tokenInfos = await nftsOwned.GetNftsByAddress();
        if (tokenInfos != null)
        {
            UpdateDisplay();
        }
    }
    private void OnDestroy()
    {
        ClearDisplay();
    }

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

    private void ClearDisplay()
    {
        //Clear display
        int childCount = content.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Destroy(content.GetChild(i).gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventory();
        }
    }

    private void ToggleInventory()
    {
        var active = !parent.activeInHierarchy;
        parent.SetActive(active);
        ExampleUIEvents.OnShowCursor.Invoke(active);
    }
}
