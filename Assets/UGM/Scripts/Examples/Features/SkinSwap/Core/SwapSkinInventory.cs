using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UGM.Core;
using UGM.Examples.Inventory;
using UGM.Examples.WeaponController;
using UnityEngine;

public class SwapSkinInventory : Inventory
{
    public WeaponType filterByWeaponType;

    private void OnEnable()
    {
        ExampleUIEvents.OnWeaponDeterminedType.AddListener(OnGetWeaponType);
    }

    private void OnDisable()
    {
        ExampleUIEvents.OnWeaponDeterminedType.RemoveListener(OnGetWeaponType);
    }

    public override async void Start()
    {
        OnGetWeaponType(WeaponType.Melee);
    }

    private async void OnGetWeaponType(WeaponType type)
    {
        filterByWeaponType = type;
        await GetTokenDataList();
    }
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

        FilterTokenInfoListByWeaponType(filteredTokenInfos);
        
        if(tokenInfos != null)
            UpdateDisplay();
    }

    private void FilterTokenInfoListByWeaponType(List<UGMDataTypes.TokenInfo> filteredTokenInfos)
    {
        foreach (var tokenInfo in filteredTokenInfos)
        {
            foreach (var attr in tokenInfo.metadata.attributes)
            {
                if (string.Equals(attr.trait_type, "Weapon Type", StringComparison.Ordinal))
                {
                    if (System.Enum.TryParse(attr.value.ToString(), out WeaponType type))
                        if (type == filterByWeaponType)
                            tokenInfos.Add(tokenInfo);
                }
            }
        }
    }
}