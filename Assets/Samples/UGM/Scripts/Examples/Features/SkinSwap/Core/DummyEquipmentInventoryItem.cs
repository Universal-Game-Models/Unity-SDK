using UGM.Examples.Inventory.InventoryItems;
using UGM.Examples.ThirdPersonController;
using UnityEngine;

namespace UGM.Examples.Features.SkinSwap.Core
{
    public class DummyEquipmentInventoryItem : InventoryItem
    {
        
        protected override void DoAction()
        {
            ExampleUIEvents.OnChangeEquipment.Invoke(tokenInfo);
        }

        protected override void DoSecondaryAction()
        {
            base.DoSecondaryAction();
        }
    }
}