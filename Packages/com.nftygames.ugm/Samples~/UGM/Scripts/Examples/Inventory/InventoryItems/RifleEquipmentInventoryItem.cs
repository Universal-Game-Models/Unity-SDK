namespace UGM.Examples.Inventory.InventoryItems
{
    /// <summary>
    /// Represents a hand equipment inventory item with additional functionality.
    /// Inherits from the base class InventoryItem and implements the IPointerClickHandler interface.
    /// Overrides the DoAction() method to add custom behavior.
    /// Implements the OnPointerClick() method to handle pointer click events.
    /// </summary>
    public class RifleEquipmentInventoryItem : HandEquipmentInventoryItem
    {
        /// <summary>
        /// Overrides the base class method to perform custom actions when the item is interacted with.
        /// Calls the base implementation first and then changes the equipment model to the secondary hand.
        /// </summary>
        protected override void DoSecondaryAction()
        {
            ChangeEquipmentModel(0);
        }
    }
}
