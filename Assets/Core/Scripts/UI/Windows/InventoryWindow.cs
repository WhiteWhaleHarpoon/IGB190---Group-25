using UnityEngine;

/// <summary>
/// InventoryWindow class handles all logic for the inventory window, such as showing the
/// individual inventory slots and redrawing as appropriate.
/// </summary>
public class InventoryWindow : UIWindow
{
    [SerializeField] private RectTransform inventoryContainer;
    private EquipmentSlot[] inventorySlots;

    /// <summary>
    /// Set up the inventory window the first time it is opened.
    /// </summary>
    private void OnEnable()
    {
        if (inventorySlots != null) return;
        inventorySlots = new EquipmentSlot[Player.MAX_INVENTORY_SIZE];
        GameObject template = inventoryContainer.GetChild(0).gameObject;
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            inventorySlots[i] = Instantiate(template, inventoryContainer).GetComponent<EquipmentSlot>();
            inventorySlots[i].Setup(GameManager.player.inventory, i);
        }
    }
}
