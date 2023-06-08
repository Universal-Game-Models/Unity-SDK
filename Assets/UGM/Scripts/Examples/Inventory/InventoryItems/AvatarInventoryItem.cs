/// <summary>
/// Represents an avatar inventory item with additional functionality.
/// Inherits from the base class InventoryItem.
/// Overrides the DoAction() method to add custom behavior.
/// </summary>
public class AvatarInventoryItem : InventoryItem
{
    AvatarLoader avatarLoader;

    /// <summary>
    /// Called when the object becomes enabled and active.
    /// Finds an instance of the AvatarLoader component in the scene.
    /// (Note: This is not considered good practice for production. Consider creating a static reference to your player instead.)
    /// </summary>
    private void OnEnable()
    {
        //Not good practice for production, create a static reference to your player
        avatarLoader = FindObjectOfType<AvatarLoader>();
    }
    /// <summary>
    /// Overrides the base class method to perform custom actions when the item is interacted with.
    /// Calls the base implementation first and then loads the avatar asynchronously using the AvatarLoader component.
    /// </summary>
    protected override void DoAction()
    {
        base.DoAction();
        if (avatarLoader)
        {
            avatarLoader.LoadAsync(tokenInfo.token_id);
        }
    }
}
