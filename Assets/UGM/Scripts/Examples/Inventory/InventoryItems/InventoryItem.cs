using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UGMDataTypes;

/// <summary>
/// Represents an item in the inventory UI.
/// Handles the initialization, display, and interaction of inventory items.
/// Implements the necessary interfaces to respond to pointer events.
/// This class can be used as a parent class to create custom inventory items with unique behaviors.
/// </summary>
public class InventoryItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    /// <summary>
    /// Reference to the Button component of the inventory item.
    /// </summary>
    [SerializeField]
    protected Button button;
    /// <summary>
    /// Reference to the TMPro.TextMeshProUGUI component for displaying the name of the inventory item.
    /// </summary>
    [SerializeField]
    private TMPro.TextMeshProUGUI nameText;
    /// <summary>
    /// The TokenInfo object associated with the inventory item.
    /// </summary>
    protected TokenInfo tokenInfo;

    /// <summary>
    /// Flag indicating if the mouse pointer is hovering over the inventory item.
    /// </summary>
    private bool hovering = false;

    /// <summary>
    /// Method to be overridden for custom actions associated with the inventory item.
    /// </summary>
    protected virtual void DoAction(){}

    /// <summary>
    /// Called when the inventory item is disabled.
    /// </summary>
    protected virtual void OnDisable()
    {
        hovering = false;
    }

    /// <summary>
    /// Initializes the inventory item with the specified TokenInfo.
    /// </summary>
    /// <param name="tokenInfo">The TokenInfo object associated with the inventory item.</param>
    public void Init(TokenInfo tokenInfo)
    {
        this.tokenInfo = tokenInfo;
        if (nameText) nameText.text = this.tokenInfo.metadata.name;
    }

    /// <summary>
    /// Called when the inventory item is started.
    /// Adds a listener to the button click event.
    /// </summary>
    private void Start()
    {
        button.onClick.AddListener(DoAction);
    }
    /// <summary>
    /// Called when the inventory item is destroyed.
    /// Removes the listener from the button click event.
    /// </summary>
    private void OnDestroy()
    {
        button.onClick.RemoveListener(DoAction);
    }

    /// <summary>
    /// Called every frame.
    /// Checks if the mouse is hovering over the inventory item and updates the quick select if a number key is pressed.
    /// </summary>
    private void Update()
    {
        if (hovering)
        {
            var numberKeyPressed = QuickSelectControl.Instance.GetNumberKeyPressed();
            if(numberKeyPressed >= 0)
            {
                QuickSelectControl.Instance.SetQuickSelect(numberKeyPressed, tokenInfo, DoAction);
            }
        }
    }

    /// <summary>
    /// Called when the mouse pointer enters the inventory item.
    /// </summary>
    /// <param name="eventData">The pointer event data.</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        hovering = true;
    }

    /// <summary>
    /// Called when the mouse pointer exits the inventory item.
    /// </summary>
    /// <param name="eventData">The pointer event data.</param>
    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;
    }
}
