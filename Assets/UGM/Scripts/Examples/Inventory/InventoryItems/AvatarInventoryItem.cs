namespace UGM.Examples.Inventory.InventoryItems
{
    /// <summary>
    /// Represents an avatar inventory item with additional functionality.
    /// Inherits from the base class InventoryItem.
    /// Overrides the DoAction() method to add custom behavior.
    /// </summary>
    public class AvatarInventoryItem : InventoryItem
    {
        /// <summary>
        /// Overrides the base class method to perform custom actions when the item is interacted with.
        /// Calls the base implementation first and then loads the avatar asynchronously using the AvatarLoader component.
        /// </summary>
        protected override void DoAction()
        {
            base.DoAction();
            if (inventory.avatarLoader)
            {
                inventory.avatarLoader.LoadAsync(tokenInfo.token_id);
            }
        }
    }
}
