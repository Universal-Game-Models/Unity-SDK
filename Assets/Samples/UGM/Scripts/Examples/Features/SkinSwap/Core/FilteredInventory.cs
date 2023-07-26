using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UGM.Core;
using UGM.Examples.Features.SkinSwap.Core;
using UGM.Examples.Inventory;
using UGM.Examples.WeaponController;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public struct FilterItem
{
    public string name;
    public bool isNameTraitType;
}
public class FilteredInventory : Inventory
{
    [field: SerializeField] [Tooltip("The Gameobject of where the reskinning happen.")]
    public SkinSwapLoader SkinSwapLoader { get; set; }
    [Tooltip("An array of filter to request")]
    public FilterItem[] filterItems;
    
    public override async void Start()
    {
        await GetTokenDataList();
    }
    /// <summary>
    /// Get all the token info from the api
    /// </summary>
    private async Task GetTokenDataList()
    {
        if (tokenInfos != null)
            tokenInfos.Clear();
        else
            tokenInfos = new List<UGMDataTypes.TokenInfo>();
        List<UGMDataTypes.TokenInfo> filteredTokenInfos = new List<UGMDataTypes.TokenInfo>();
        if (nftsOwned == null)
        {
            Debug.LogError("nftsOwned is null.");
            return;
        }

        filteredTokenInfos = await nftsOwned.GetNftsByAddress();

        FilterTokenInfoList(filteredTokenInfos);
        
        if(tokenInfos != null)
            UpdateDisplay();
    }

    public override void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            ToggleInventory();
        }
    }

    /// <summary>
    /// Filter the list of tokeninfos using the array of filter request.
    /// </summary>
    /// <param name="filteredTokenInfos"></param>
    private void FilterTokenInfoList(List<UGMDataTypes.TokenInfo> filteredTokenInfos)
    {
        foreach (var tokenInfo in filteredTokenInfos)
        {
            foreach (var filterType in filterItems)
            {
                UGMDataTypes.Attribute attribute = GetFilteredAttribute(tokenInfo, filterType);
                if (IsAttributeFiltered(attribute))
                {
                    AddTokenInfoToListOfInventory(tokenInfo);
                    break;
                }
            }
        }
    }

    private UGMDataTypes.Attribute GetFilteredAttribute(UGMDataTypes.TokenInfo tokenInfo, FilterItem filterType)
    {
        return tokenInfo.metadata.attributes.FirstOrDefault(md =>
            filterType.isNameTraitType
                ? md.trait_type == filterType.name
                : md.value.ToString() == filterType.name);
    }

    private void AddTokenInfoToListOfInventory(UGMDataTypes.TokenInfo tokenInfo)
    {
        tokenInfos.Add(tokenInfo);
    }

    private bool IsAttributeFiltered(UGMDataTypes.Attribute attribute)
    {
        return attribute != null;
    }
}