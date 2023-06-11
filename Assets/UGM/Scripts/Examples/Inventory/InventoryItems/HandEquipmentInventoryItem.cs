using UnityEngine.EventSystems;

/// <summary>
/// Represents a hand equipment inventory item with additional functionality.
/// Inherits from the base class InventoryItem and implements the IPointerClickHandler interface.
/// Overrides the DoAction() method to add custom behavior.
/// Implements the OnPointerClick() method to handle pointer click events.
/// </summary>
public class HandEquipmentInventoryItem : InventoryItem, IPointerClickHandler
{
    private HumanoidEquipmentLoader[] tools;

    /// <summary>
    /// Called when the object becomes enabled and active.
    /// Finds an instance of the AvatarLoader component in the scene and retrieves the HumanoidEquipmentLoader components.
    /// </summary>
    public override void Init(Inventory inventory, UGMDataTypes.TokenInfo tokenInfo)
    {
        base.Init(inventory, tokenInfo);
        if (inventory.avatarLoader) tools = inventory.avatarLoader.GetComponentsInChildren<HumanoidEquipmentLoader>();
    }

    /// <summary>
    /// Overrides the base class method to perform custom actions when the item is interacted with.
    /// Calls the base implementation first and then changes the equipment model to the default hand.
    /// </summary>
    protected override void DoAction()
    {
        base.DoAction();
        ChangeEquipmentModel(0);
    }

    /// <summary>
    /// Implements the IPointerClickHandler interface method to handle pointer click events.
    /// Changes the equipment model to the alternate hand when the right mouse button is clicked.
    /// </summary>
    /// <param name="eventData">The pointer event data associated with the click.</param>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            ChangeEquipmentModel(1);
        }
    }

    /// <summary>
    /// Changes the equipment model to the specified hand.
    /// Loads the equipment asynchronously using the HumanoidEquipmentLoader component and the token ID.
    /// </summary>
    private void ChangeEquipmentModel(int handIndex)
    {
        if (tools != null && tools.Length > handIndex)
        {
            tools[handIndex].LoadAsync(tokenInfo.token_id);
        }
    }
}
