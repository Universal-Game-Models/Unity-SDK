using UGM.Core;
using UGM.Examples.ThirdPersonController;

namespace UGM.Examples.Inventory.InventoryItems
{
    /// <summary>
    /// Represents a hand equipment inventory item with additional functionality.
    /// Inherits from the base class InventoryItem and implements the IPointerClickHandler interface.
    /// Overrides the DoAction() method to add custom behavior.
    /// Implements the OnPointerClick() method to handle pointer click events.
    /// </summary>
    public class HandEquipmentInventoryItem : InventoryItem
    {
        protected HumanoidEquipmentLoader[] tools;

        /// <summary>
        /// Called when the object becomes enabled and active.
        /// Finds an instance of the AvatarLoader component in the scene and retrieves the HumanoidEquipmentLoader components.
        /// </summary>
        public override void Init(global::UGM.Examples.Inventory.Inventory inventory, UGMDataTypes.TokenInfo tokenInfo)
        {
            base.Init(inventory, tokenInfo);
            if (inventory.avatarLoader) tools = inventory.avatarLoader.GetComponentsInChildren<HumanoidEquipmentLoader>();
        }

        /// <summary>
        /// Overrides the base class method to perform custom actions when the item is interacted with.
        /// Calls the base implementation first and then changes the equipment model to the primary hand.
        /// </summary>
        protected override void DoAction()
        {
            base.DoAction();
            ChangeEquipmentModel(0);
        }

        /// <summary>
        /// Overrides the base class method to perform custom actions when the item is interacted with.
        /// Calls the base implementation first and then changes the equipment model to the secondary hand.
        /// </summary>
        protected override void DoSecondaryAction()
        {
            base.DoSecondaryAction();
            ChangeEquipmentModel(1);
        }

        /// <summary>
        /// Changes the equipment model to the specified hand or destroys it if it is equipped.
        /// Loads the equipment asynchronously using the HumanoidEquipmentLoader component and the token ID.
        /// </summary>
        protected void ChangeEquipmentModel(int handIndex)
        {
            if (tools != null && tools.Length > handIndex)
            {
                tools[handIndex].ToggleLoad(tokenInfo.token_id);
            }
        }
    }
}
