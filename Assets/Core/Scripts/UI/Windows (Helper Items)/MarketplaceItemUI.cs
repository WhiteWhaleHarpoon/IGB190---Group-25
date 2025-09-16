using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the UI for an individual item in the marketplace, including display and selection logic.
/// </summary>
public class MarketplaceItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI itemName;
    [SerializeField] private TextMeshProUGUI itemCost;
    [SerializeField] private Image itemIcon;
    [SerializeField] private Image selectedFrame;
    [SerializeField] private Image outlineFrame;

    [HideInInspector] public Item attachedItem;

    /// <summary>
    /// Sets up the UI for the given item.
    /// </summary>
    /// <param name="item">The item to display in this UI element.</param>
    public void Setup(Item item)
    {
        attachedItem = item;
        itemName.text = item.name;
        itemCost.text = GameManager.player.currentGold >= item.itemCost
            ? item.itemCost.ToString()
            : $"<color=red>{item.itemCost}</color>";

        itemIcon.sprite = item.itemIcon;
        Color rarityColor = item.GetItemColor();
        itemName.color = rarityColor;
        outlineFrame.color = rarityColor;

        gameObject.SetActive(true);
    }

    /// <summary>
    /// Sets the visual state of this item to reflect its selection status.
    /// </summary>
    /// <param name="isSelected">Whether this item is selected.</param>
    public void SetSelected(bool isSelected)
    {
        selectedFrame.enabled = isSelected;
    }

    /// <summary>
    /// Selects this item in the shop if the player can afford it.
    /// </summary>
    public void SetSelectedShopItem()
    {
        if (GameManager.player.currentGold >= attachedItem.itemCost)
        {
            GameManager.ui.ShopWindow.SetSelectedMarketplaceItem(this);
        }
        else
        {
            GameManager.ui.ShopWindow.SetSelectedMarketplaceItem(null);
        }
    }

    /// <summary>
    /// Displays the tooltip for this item when the mouse enters the UI element.
    /// </summary>
    public void OnMouseEnterUI()
    {
        GameManager.ui.TooltipWindow.Show(attachedItem);
    }

    /// <summary>
    /// Hides the tooltip when the mouse exits the UI element.
    /// </summary>
    public void OnMouseExitUI()
    {
        GameManager.ui.TooltipWindow.Hide();
    }
}