using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Handles the logic for buying and selling items in the shop. Items are automatically
/// added and categorized by the shop without manual addition.
/// </summary>
public class ShopWindow : UIWindow
{
    // Serialized fields - assigned via the Unity Inspector
    [SerializeField] private RectTransform marketplaceContainer;
    [SerializeField] private RectTransform marketplaceItemsContainer;
    [SerializeField] private EquipmentSlot sellSlot;
    [SerializeField] private Button purchaseButton;
    [SerializeField] private TextMeshProUGUI itemPurchaseCost;
    [SerializeField] private TextMeshProUGUI currentGoldText;
    [SerializeField] private GameFeedback sellFeedback;

    // Private fields - assigned via code
    private MarketplaceItemUI selectedItemUI;
    private MarketplaceFilterUI[] marketplaceFilterSlots;
    private Inventory sellInventory;
    private Item.ItemType currentFilter;

    /// <summary>
    /// Initializes the shop window, setting up filters, inventory, and listeners.
    /// </summary>
    public override void Setup()
    {
        base.Setup();
        marketplaceFilterSlots = marketplaceContainer.GetComponentsInChildren<MarketplaceFilterUI>();
        sellInventory = new Inventory(1);
        sellSlot.Setup(sellInventory, 0);
        sellInventory.onItemAdded.AddListener(SellItem);

        GameManager.events.OnGoldAdded.AddListener(x => UpdateGoldDisplay());
        GameManager.events.OnGoldRemoved.AddListener(x => UpdateGoldDisplay());

        purchaseButton.onClick.AddListener(() => PurchaseItem(selectedItemUI?.attachedItem));
        UpdateGoldDisplay();
    }

    /// <summary>
    /// Applies a default filter when the shop window is enabled.
    /// </summary>
    private void OnEnable()
    {
        ApplyMarketplaceFilter(Item.ItemType.Weapon);
    }

    /// <summary>
    /// Sells the specified item, awarding the player with gold and triggering related events.
    /// </summary>
    private void SellItem(Item item)
    {
        GameManager.player.AddGold(Mathf.Round(item.itemCost * 0.5f));
        sellInventory.RemoveItem(item);
        GameManager.events.OnItemSold.Invoke(item);
        sellFeedback.ActivateFeedback();
    }

    /// <summary>
    /// Filters and updates the marketplace items based on the specified item type.
    /// </summary>
    public void ApplyMarketplaceFilter(Item.ItemType filter)
    {
        currentFilter = filter;

        // Update filter selection state
        foreach (var filterSlot in marketplaceFilterSlots)
        {
            filterSlot.SetSelected(filter == filterSlot.filter);
        }

        // Clear existing marketplace items
        ClearMarketplaceItems();

        // Recreate marketplace items based on the selected filter
        CreateMarketplaceItems(filter);

        // Hide the purchase button initially
        purchaseButton.gameObject.SetActive(false);
    }

    /// <summary>
    /// Clears the marketplace item slots, keeping the template intact.
    /// </summary>
    private void ClearMarketplaceItems()
    {
        for (int i = marketplaceItemsContainer.childCount - 1; i >= 1; i--)
        {
            Destroy(marketplaceItemsContainer.GetChild(i).gameObject);
        }
    }

    /// <summary>
    /// Creates and populates the marketplace with items based on the selected filter.
    /// </summary>
    private void CreateMarketplaceItems(Item.ItemType filter)
    {
        var template = marketplaceItemsContainer.GetChild(0).GetComponent<MarketplaceItemUI>();

        foreach (var item in Item.GetAllItemsOfType(filter))
        {
            if (item.canPurchaseInShop && (item.classRequirement == "" || item.classRequirement == GameManager.player.unitName))
            {
                Instantiate(template, marketplaceItemsContainer).Setup(item);
            }
        }
    }

    /// <summary>
    /// Marks the specified marketplace item as selected and updates the UI accordingly.
    /// </summary>
    public void SetSelectedMarketplaceItem(MarketplaceItemUI item)
    {
        if (item == null) return;

        selectedItemUI = item;
        itemPurchaseCost.text = item.attachedItem.itemCost.ToString();
        purchaseButton.gameObject.SetActive(true);

        foreach (var slot in marketplaceItemsContainer.GetComponentsInChildren<MarketplaceItemUI>())
        {
            slot.SetSelected(slot == item);
        }
    }

    /// <summary>
    /// Purchases the selected item, deducting gold and adding the item to the player's inventory.
    /// </summary>
    public void PurchaseItem(Item item)
    {
        if (item == null) return;

        GameManager.player.inventory.AddItem(item.RollItem());
        GameManager.player.RemoveGold(item.itemCost);

        selectedItemUI.SetSelected(false);
        purchaseButton.gameObject.SetActive(false);

        GameManager.logicEngine.TriggerEventOnAllEngines(null, LogicEngine.EVENT_ITEM_BOUGHT);
    }

    /// <summary>
    /// Updates the displayed gold amount and refreshes the marketplace filter.
    /// </summary>
    private void UpdateGoldDisplay()
    {
        currentGoldText.text = GameManager.player.currentGold.ToString();
        ApplyMarketplaceFilter(currentFilter);
    }
}