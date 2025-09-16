using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controls the logic for a Gold Pickup. A gold pickup is an object in the world
/// that stores a certain amount of gold. If the player collects the pickup, they gain
/// the gold and the pickup is destroyed.
/// </summary>
public class GoldPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    public float pickupDistance = 2.5f;
    public int goldAmount = 100;
    public GameFeedback pickupFeedback;

    [Header("UI References")]
    public RectTransform pickupUITransform;
    public TextMeshProUGUI pickupUILabel;
    public Image pickupUIBackground;
    public CanvasGroup pickupUICanvasGroup;

    private const float PICKUP_DISPLAY_DISTANCE = 5.0f;

    /// <summary>
    /// Initializes the gold pickup with the specified amount of gold.
    /// </summary>
    public void Setup(int goldAmount)
    {
        this.goldAmount = goldAmount;
        pickupUILabel.text = $"{goldAmount} Gold";
    }

    /// <summary>
    /// Updates the gold pickup every frame.
    /// </summary>
    private void Update()
    {
        RefreshUIDisplay();

        if (CanPickupGold())
        {
            PickupGold();
        }
    }

    /// <summary>
    /// Determines if the player is close enough to pick up the gold.
    /// </summary>
    private bool CanPickupGold()
    {
        float distanceToGold = Vector3.Distance(transform.position, GameManager.player.transform.position);
        return distanceToGold < pickupDistance && GameManager.player.IsMoving();
    }

    /// <summary>
    /// Determines if the pickup UI should be visible based on player distance and settings.
    /// </summary>
    private bool IsPickupUIVisible()
    {
        float distanceToGold = Vector3.Distance(transform.position, GameManager.player.transform.position);
        return GameManager.settings.showGoldPickupUI && distanceToGold < PICKUP_DISPLAY_DISTANCE;
    }

    /// <summary>
    /// Handles the gold pickup process, giving the gold to the player and triggering related actions.
    /// </summary>
    private void PickupGold()
    {
        GameManager.player.AddGold(goldAmount);
        pickupFeedback?.ActivateFeedback(null, null, transform.position);
        GameManager.events.OnGoldPickedUp.Invoke(goldAmount);
        ObjectPooler.DestroyPooled(gameObject);
        StatusMessageUI.Spawn(transform.position + Vector3.up, $"+{goldAmount} Gold", Color.yellow);
    }

    /// <summary>
    /// Updates the pickup's UI display based on its visibility.
    /// </summary>
    private void RefreshUIDisplay()
    {
        if (IsPickupUIVisible())
        {
            RefreshUIPosition();
        }
        else
        {
            pickupUITransform.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Positions the UI correctly in world space.
    /// </summary>
    private void RefreshUIPosition()
    {
        pickupUITransform.gameObject.SetActive(true);
        Vector2 screenPosition = Camera.main.WorldToScreenPoint(transform.position + Vector3.up);
        screenPosition.x /= pickupUITransform.transform.parent.localScale.x;
        screenPosition.y /= pickupUITransform.transform.parent.localScale.y;
        pickupUITransform.anchoredPosition = screenPosition;
    }

    /// <summary>
    /// Spawns a gold pickup at the specified location with the specified amount of gold.
    /// </summary>
    public static void Spawn(Vector3 position, int goldAmount)
    {
        GameObject obj = ObjectPooler.InstantiatePooled(GameManager.assets.goldPickup.gameObject, position, Quaternion.identity);
        obj.GetComponent<GoldPickup>().Setup(goldAmount);
    }
}