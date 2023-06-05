using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static UGMManager;
using static UGMDataTypes;
using UnityEngine.EventSystems;

public class ModelInventory : MonoBehaviour
{
    public GetModelsOwned modelsOwned;

    [SerializeField]
    private GameObject parent;
    [SerializeField]
    private Transform content;
    [SerializeField]
    private ModelItem defaultItemPrefab;
    [SerializeField]
    private ItemPrefabs[] itemPrefabs;

    [Serializable]
    public struct ItemPrefabs
    {
        public string name;
        public bool nameIsTraitType;
        public ModelItem prefab;
    }

    private List<ModelsOwnedTokenInfo> tokenInfos;

    // Start is called before the first frame update
    async void Start()
    {
        //Load the data
        tokenInfos = await modelsOwned.GetModelsOwnedByAddress();
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
            //Could add a switch case for different item types
            //They could use custom ModelItems that have a unique action
            ModelItem prefab = null;
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
        parent.SetActive(!parent.activeInHierarchy);
    }
}
