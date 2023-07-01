using UGM.Examples.Inventory.InventoryItems.Controls;

namespace UGM.Examples.Inventory.InventoryItems
{
    /// <summary>
    /// Represents an instantiatable inventory item with additional functionality.
    /// Inherits from the base class InventoryItem.
    /// Overrides the DoAction() method to add custom behavior.
    /// </summary>
    public class InstantiatableInventoryItem : InventoryItem
    {
        /// <summary>
        /// Overrides the base class method to perform custom actions when the item is interacted with.
        /// Calls the base implementation first and then sets the token information using InstantiatableInventoryControl.
        /// </summary>
        protected override void DoAction()
        {
            base.DoAction();
            InstantiatableInventoryControl.Instance.SetTokenInfo(tokenInfo);
        }
    }
}
