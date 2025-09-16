using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// Manages the display and interaction of an equipment slot in the player's inventory.
/// </summary>
public class EquipmentSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Image iconBackground;
    [SerializeField] private Image icon;
    [SerializeField] private Image rarity;

    private RectTransform iconTransform;
    private Item attachedItem;
    private bool isDraggingItem;
    private RectTransform parent;
    private Inventory inventory;
    private int inventorySlotID;

    [SerializeField] private bool applyFilter = true;
    [SerializeField] private Item.ItemType allowedItemTypes;

    private const float DRAG_THRESHOLD = 60.0f;

    /// <summary>
    /// Sets up the equipment slot with the specified inventory and slot ID.
    /// </summary>
    public void Setup(Inventory inventory, int id)
    {
        gameObject.SetActive(true);
        this.inventory = inventory;
        inventorySlotID = id;
        iconTransform = iconBackground.GetComponent<RectTransform>();
        parent = GetComponent<RectTransform>();

        Redraw();
        inventory.onSlotUpdated.AddListener(x =>
        {
            if (x == inventorySlotID)
            {
                Redraw();
            }
        });
    }

    /// <summary>
    /// Redraws the equipment slot to reflect the current item.
    /// </summary>
    public void Redraw()
    {
        attachedItem = inventory.GetItemAtID(inventorySlotID);
        if (attachedItem == null)
        {
            iconBackground.gameObject.SetActive(false);
        }
        else
        {
            iconBackground.gameObject.SetActive(true);
            icon.enabled = true;
            icon.sprite = attachedItem.itemIcon;
            rarity.color = attachedItem.RarityToColor(attachedItem.itemRarity);
        }
    }

    /// <summary>
    /// If the user is dragging the equipment slot, update its position every frame.
    /// </summary>
    private void Update()
    {
        if (isDraggingItem)
        {
            iconTransform.position = Input.mousePosition;
        }
    }

    /// <summary>
    /// When the user hovers over an equipment slot, show a tooltip if something is in the slot.
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (attachedItem != null)
        {
            GameManager.ui.TooltipWindow.Show(attachedItem);
        }
    }

    /// <summary>
    /// When the user stops hovering over an equipment slot, hide the displayed tooltip.
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        GameManager.ui.TooltipWindow.Hide();
    }

    /// <summary>
    /// When the user starts a press on an item, begin a drag action.
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        if (attachedItem != null)
        {
            iconTransform.SetParent(GetComponentInParent<Canvas>().transform);
            isDraggingItem = true;
        }
    }

    /// <summary>
    /// When the user stops a press on an item, trigger a swap between the two slots (or reset
    /// the drag if a valid item is not found).
    /// </summary>
    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isDraggingItem) return;

        Item.ItemType type = attachedItem.itemType;
         
        EquipmentSlot closestSlot = FindClosestValidSlot();

        if (closestSlot != null)
        {
            if ((closestSlot.allowedItemTypes == attachedItem.itemType || !closestSlot.applyFilter) &&
            (closestSlot.attachedItem == null || allowedItemTypes == closestSlot.attachedItem.itemType || !applyFilter))
            {
                SwapItemsWith(closestSlot);
            }
        }

        ResetDraggingState();
    }

    /// <summary>
    /// Finds the closest valid equipment slot that can accept the dragged item.
    /// </summary>
    private EquipmentSlot FindClosestValidSlot()
    {
        float closestDistance = DRAG_THRESHOLD;
        EquipmentSlot closestSlot = null;
        EquipmentSlot[] slots = FindObjectsByType<EquipmentSlot>(FindObjectsSortMode.None);

        foreach (var slot in slots)
        {
            float dist = Vector2.Distance(icon.transform.position, slot.transform.position);
            if (dist < closestDistance && (!slot.applyFilter || slot.allowedItemTypes == attachedItem.itemType))
            {
                closestDistance = dist;
                closestSlot = slot;
            }
        }

        return closestSlot;
    }

    /// <summary>
    /// Swaps the item in this slot with the item in the specified slot.
    /// </summary>
    private void SwapItemsWith(EquipmentSlot otherSlot)
    {
        Item itemInOtherSlot = otherSlot.inventory.GetItemAtID(otherSlot.inventorySlotID);
        Item inThisSlot = inventory.GetItemAtID(inventorySlotID);

        // Add the items to their new slots
        otherSlot.inventory.AddItemAtID(inThisSlot, otherSlot.inventorySlotID);
        inventory.AddItemAtID(itemInOtherSlot, inventorySlotID);

        // Redraw the slots to reflect the change
        otherSlot.Redraw();
        Redraw();
    }

    /// <summary>
    /// Resets the state of the slot after dragging, returning the item to its original position.
    /// </summary>
    private void ResetDraggingState()
    {
        iconTransform.SetParent(parent);
        iconTransform.anchoredPosition = Vector2.zero;
        isDraggingItem = false;
    }
}