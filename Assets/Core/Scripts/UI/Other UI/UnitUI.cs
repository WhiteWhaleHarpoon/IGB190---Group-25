using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the UI elements related to a unit, such as the health bar and name display.
/// </summary>
public class UnitUI : MonoBehaviour
{
    [SerializeField] private Image healthBar;
    [SerializeField] private Vector3 offset;
    [SerializeField] private RectTransform container;
    [SerializeField] private TextMeshProUGUI unitName;
    [SerializeField] private GameObject empoweredFrame;

    private Unit unit;

    /// <summary>
    /// Determines if the health bar should be visible based on the unit type and settings.
    /// </summary>
    private bool IsHealthBarVisible()
    {
        if (unit is not Monster monster)
            return true;

        return GameManager.settings.showFullHealthBars || monster.isEmpowered || unit.health < unit.stats.GetValue(Stat.MaxHealth);
    }

    /// <summary>
    /// Updates the position and fill amount of the health bar based on the unit's current health.
    /// </summary>
    private void UpdateHealthBar()
    {
        container.gameObject.SetActive(IsHealthBarVisible());

        Vector2 screenPoint = Camera.main.WorldToScreenPoint(unit.transform.position + offset);
        screenPoint.x /= transform.parent.localScale.x;
        screenPoint.y /= transform.parent.localScale.y;
        container.anchoredPosition = screenPoint;

        healthBar.fillAmount = unit.health / unit.stats.GetValue(Stat.MaxHealth);
    }

    /// <summary>
    /// Handles the late frame update, ensuring the health bar is updated or the UI is destroyed.
    /// </summary>
    private void LateUpdate()
    {
        if (unit != null)
        {
            UpdateHealthBar();
        }
        else
        {
            ObjectPooler.DestroyPooled(gameObject);
        }
    }

    /// <summary>
    /// Spawns a UI for the specified unit, setting up the health bar and name display.
    /// </summary>
    public static void Spawn(Unit unit, float heightOffset = 2.0f, float scale = 1.0f)
    {
        GameObject obj = ObjectPooler.InstantiatePooled(GameManager.assets.unitUI.gameObject, Vector3.zero, Quaternion.identity);
        obj.transform.SetParent(GameManager.ui.DynamicCanvas.transform);

        UnitUI unitUI = obj.GetComponent<UnitUI>();
        unitUI.offset = Vector3.up * heightOffset;
        unitUI.unit = unit;
        unitUI.transform.localScale = Vector3.one;
        unitUI.container.transform.localScale = new Vector3(scale, scale, scale);

        unitUI.SetupUnitUI();
        unitUI.UpdateHealthBar();
    }

    /// <summary>
    /// Configures the UI based on the unit's properties (e.g., empowered state, name visibility).
    /// </summary>
    private void SetupUnitUI()
    {
        if (unit is Monster monster && monster.healthBarType == Monster.HealthBarType.Empowered)
        {
            container.gameObject.SetActive(true);
            empoweredFrame.SetActive(true);
            unitName.gameObject.SetActive(true);
            unitName.text = monster.unitName;
        }
        else
        {
            container.gameObject.SetActive(false);
            empoweredFrame.SetActive(false);
            unitName.gameObject.SetActive(false);
            unitName.text = string.Empty;
        }
        if (unit.GetFaction() == Unit.Faction.Player)
            healthBar.color = new Color(0.5f, 0.5f, 1.0f);
    }
}