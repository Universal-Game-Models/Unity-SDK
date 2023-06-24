using UGM.Core;
using UGM.Examples.Features.SkinSwap.Interface;
using UGM.Examples.Inventory.InventoryItems;
using UnityEngine;

namespace UGM.Examples.Features.SkinSwap.Core
{
    public class SkinSwapInventoryItem : HandEquipmentInventoryItem
    {
        private ILoadableSkin loader;
        protected override void Update()
        {
            
        }

        protected override void DoAction()
        {
            base.DoAction();
            if (loader == null)
            {
                Debug.LogError("Swap Loader is null");
                return;
            }

            loader.LoadItem(tokenInfo);
        }

        public override void Init(Inventory.Inventory inventory, UGMDataTypes.TokenInfo tokenInfo)
        {
            base.Init(inventory, tokenInfo);
            FilteredInventory swapInventory = (FilteredInventory)inventory;
            loader = swapInventory.SkinSwapLoader;
            this.tokenInfo = tokenInfo;
        }
    }
}