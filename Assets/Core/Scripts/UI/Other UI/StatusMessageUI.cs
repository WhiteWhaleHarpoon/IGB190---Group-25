using TMPro;
using UnityEngine;

/// <summary>
/// Handles the creation and management of generic status messages that can be placed into the world.
/// </summary>
public class StatusMessageUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI content;

    private Vector3 lockPosition;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private float spawnedAt;

    private const float MessageTime = 1.0f;
    private const float HeightChange = 2.0f;

    /// <summary>
    /// Spawns a status message with the given text, color, and size at the specified world position.
    /// </summary>
    public static void Spawn(Vector3 position, string text, Color color, float scale = 1.0f)
    {
        StatusMessageUI statusMessage = Instantiate(GameManager.assets.statusMessageUI,
            GameManager.ui.DynamicCanvas.transform);

        statusMessage.rectTransform = statusMessage.GetComponent<RectTransform>();
        statusMessage.canvasGroup = statusMessage.GetComponent<CanvasGroup>();
        statusMessage.content.text = text;
        statusMessage.content.color = color;
        statusMessage.lockPosition = position;
        statusMessage.spawnedAt = Time.time;
        statusMessage.content.transform.localScale *= scale;

        statusMessage.UpdateVisual();
    }

    /// <summary>
    /// Handles the movement and visibility of the status message over time.
    /// </summary>
    private void Update()
    {
        UpdateVisual();

        if (Time.time > (spawnedAt + MessageTime))
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Updates the visual appearance of the status message, including position and transparency.
    /// </summary>
    private void UpdateVisual()
    {
        float elapsedRatio = (Time.time - spawnedAt) / MessageTime;
        Vector2 screenPosition = Camera.main.WorldToScreenPoint(lockPosition);

        screenPosition += new Vector2(0, elapsedRatio * HeightChange);
        screenPosition.x /= transform.parent.localScale.x;
        screenPosition.y /= transform.parent.localScale.y;

        rectTransform.anchoredPosition = screenPosition;
        canvasGroup.alpha = 1.0f - elapsedRatio;
    }
}