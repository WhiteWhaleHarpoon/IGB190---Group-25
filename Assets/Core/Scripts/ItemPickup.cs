using MyUtilities;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using EditorAttributes;

public class ItemPickup : Interactable
{
    [Header("Pickup Data")]
    public bool spawnRandomItem;
    [DisableField("spawnRandomItem")] public Item item;
    [EnableField("spawnRandomItem")] public Item.ItemRarity randomSpawnRarity;
    public GameFeedback pickupFeedback;

    [SerializeField, HideInInspector] private float uiHeight = 0.5f;
    [SerializeField, HideInInspector] private RectTransform pickupUITransform;
    [SerializeField, HideInInspector] private TextMeshProUGUI pickupUILabel;
    [SerializeField, HideInInspector] private Image pickupUIBackground;
    [SerializeField, HideInInspector] private CanvasGroup pickupUICanvasGroup;

    [ToggleGroup("UI Components", nameof(uiHeight), nameof(pickupUITransform), nameof(pickupUILabel), nameof(pickupUIBackground), nameof(pickupUICanvasGroup))]
    [SerializeField] private Void groupHolder;

    private bool hoveringOver;

    /// <summary>
    /// Initializes the item pickup. If an item is not assigned or if a random item should be spawned,
    /// it assigns a random item of the specified rarity.
    /// </summary>
    private void Start()
    {
        if (item == null || spawnRandomItem)
        {
            item = Item.GetRandomItemOfRarity(randomSpawnRarity);
        }

        SetupItemUI(4);
    }

    /// <summary>
    /// Sets up the UI for the item with a specific outline thickness.
    /// </summary>
    private void SetupItemUI(int outlineThickness)
    {
        Color itemColor = item.GetItemColor();
        pickupUILabel.text = item.itemName;
        pickupUILabel.color = itemColor;
        SetOutline(itemColor, outlineThickness);
    }

    /// <summary>
    /// Handles frame-specific updates.
    /// </summary>
    protected override void Update()
    {
        base.Update();

        if (hoveringOver)
        {
            GameManager.hoveredInteractable = this;
        }

        UpdateUIPosition();
        UpdateUIAlpha();
    }

    /// <summary>
    /// Updates the position of the UI element to match the world position of the item.
    /// </summary>
    private void UpdateUIPosition()
    {
        Vector2 screenPosition = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * uiHeight);
        screenPosition.x /= pickupUITransform.transform.parent.localScale.x;
        screenPosition.y /= pickupUITransform.transform.parent.localScale.y;
        pickupUITransform.anchoredPosition = screenPosition;
    }

    /// <summary>
    /// Updates the alpha transparency of the UI element based on whether the item is hovered over.
    /// </summary>
    private void UpdateUIAlpha()
    {
        pickupUICanvasGroup.alpha = (GameManager.hoveredInteractable == this ? 1.0f : 0.5f);
    }

    /// <summary>
    /// Handles interaction when the mouse is clicked on the item.
    /// </summary>
    protected virtual void OnMouseDown()
    {
        TryToInteract();
    }

    protected void OnMouseEnter()
    {
        SelectItem();
    }

    private void OnMouseExit()
    {
        DeselectItem();
    }

    /// <summary>
    /// Marks the item as selected when the player hovers over it.
    /// </summary>
    public void SelectItem()
    {
        GameManager.hoveredInteractable = this;
        hoveringOver = true;
    }

    /// <summary>
    /// Unmarks the item as selected when the player stops hovering over it.
    /// </summary>
    public void DeselectItem()
    {
        hoveringOver = false;

        if (GameManager.hoveredInteractable == this)
        {
            GameManager.hoveredInteractable = null;
        }
    }

    /// <summary>
    /// Handles the item pickup interaction, adding the item to the player's inventory and triggering feedback.
    /// </summary>
    public override void OnInteraction()
    {
        base.OnInteraction();

        Item rolledItem = item.RollItem();
        GameManager.player.inventory.AddItem(rolledItem);
        GameManager.events.OnItemPickedUp.Invoke(rolledItem);
        pickupFeedback?.ActivateFeedback(null, null, transform.position);

        GameManager.selectedInteractable = null;

        if (ObjectPooler.IsTracked(gameObject))
        {
            ObjectPooler.DestroyPooled(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        GameManager.player.StopMoving();
    }

    /// <summary>
    /// Spawns a specific item at the given location.
    /// </summary>
    public static void Spawn(Vector3 position, Item item)
    {
        if (item == null) return;
        ItemPickup itemPickup = ObjectPooler.InstantiatePooled(GameManager.assets.itemPickup.gameObject, 
            position, Quaternion.identity).GetComponent<ItemPickup>();
        itemPickup.item = item;
        itemPickup.spawnRandomItem = false;
        itemPickup.SetupItemUI(2);
    }

    /// <summary>
    /// Spawns a random item with the given rarity at the given location.
    /// </summary>
    public static void Spawn(Vector3 position, Item.ItemRarity rarity)
    {
        Item item = Item.GetRandomItemOfRarity(rarity);
        Spawn(position, item);
    }
}
