using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles the display of tooltips for abilities and items.
/// </summary>
public class TooltipWindow : UIWindow
{
    [SerializeField] private Image tooltipIcon;            
    [SerializeField] private TextMeshProUGUI tooltipTitle;   
    [SerializeField] private TextMeshProUGUI tooltipSubheading; 
    [SerializeField] private TextMeshProUGUI tooltipDescription;
    [SerializeField] private TextMeshProUGUI tooltipFlavourText;

    /// <summary>
    /// Shows the tooltip for a given ability.
    /// </summary>
    /// <param name="ability">The ability to display in the tooltip.</param>
    public void Show(Ability ability)
    {
        tooltipIcon.sprite = ability.abilityIcon;
        tooltipTitle.text = ability.abilityName;
        tooltipTitle.color = new Color(0.8f, 0.6f, 0.5f); // Custom color for ability titles
        tooltipSubheading.text = $"<color=yellow>{ability.GetTotalCooldown(GameManager.player):N1}s Cooldown</color>";

        tooltipDescription.text = ability.GetTooltip(GameManager.player)
            .Replace("Resource", GameManager.player.resourceName);

        tooltipFlavourText.gameObject.SetActive(ability.abilityFlavourText.Length > 0);
        tooltipFlavourText.text = ability.abilityFlavourText;

        gameObject.SetActive(true);
    }

    /// <summary>
    /// Shows the tooltip for a given item.
    /// </summary>
    /// <param name="item">The item to display in the tooltip.</param>
    public void Show(Item item)
    {
        tooltipIcon.sprite = item.itemIcon;
        tooltipTitle.text = item.itemName;
        tooltipTitle.color = item.GetItemColor();
        tooltipSubheading.text = item.GetTypeDescription();
        tooltipDescription.text = $"<color=yellow>{item.GetDescription().Replace("Resource", GameManager.player.resourceName)}</color>";

        tooltipFlavourText.gameObject.SetActive(item.itemFlavourText.Length > 0);
        tooltipFlavourText.text = item.itemFlavourText;

        gameObject.SetActive(true);
    }
}
