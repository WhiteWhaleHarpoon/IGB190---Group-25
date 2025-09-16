using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles the UI for filtering items in the marketplace based on item type.
/// </summary>
public class MarketplaceFilterUI : MonoBehaviour
{
    public Item.ItemType filter;
    [SerializeField] private Image outline;
    [SerializeField] private CanvasGroup canvasGroup;

    /// <summary>
    /// Sets the visual state of the filter based on whether it is selected.
    /// </summary>
    public void SetSelected(bool isSelected)
    {
        outline.enabled = isSelected;
        canvasGroup.alpha = isSelected ? 1.0f : 0.5f;
    }

    /// <summary>
    /// Applies the filter to the marketplace, showing only items of the specified type.
    /// </summary>
    public void ApplyFilter()
    {
        GameManager.ui.ShopWindow.ApplyMarketplaceFilter(filter);
    }
}