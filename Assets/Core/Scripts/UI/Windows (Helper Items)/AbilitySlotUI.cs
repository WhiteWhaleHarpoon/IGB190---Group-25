using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// Handles the UI display and interactions for an ability slot, including showing cooldowns,
/// hotkeys, and tooltips.
/// </summary>
public class AbilitySlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image abilityIcon;
    [SerializeField] private Image reminderFlash;
    [SerializeField] private TextMeshProUGUI unlockText;
    [SerializeField] private Image cooldownSweeper;
    [SerializeField] private TextMeshProUGUI hotkeyText;
    [SerializeField] private TextMeshProUGUI cooldownText;

    private Ability ability;

    /// <summary>
    /// Sets up the ability slot with the given ability and hotkey.
    /// </summary>
    public void Setup(Ability ability, string hotkey)
    {
        this.ability = ability;

        if (ability != null)
        {
            abilityIcon.sprite = ability.abilityIcon;
            unlockText.text = $"Unlocked {ability.abilityName}";
        }

        cooldownSweeper.fillAmount = 0;
        hotkeyText.text = FormatHotkeyText(hotkey);
        
        RedrawHotkey();
    }

    public void RedrawHotkey ()
    {
        string hotkey = GameManager.settings.keybindings[GameManager.player.abilities.IndexOf(ability)].ToString();
        hotkeyText.text = FormatHotkeyText(hotkey);
        if (hotkeyText.text.Length > 1 && hotkeyText.text[0] != '<') hotkeyText.text = "";
    }

    /// <summary>
    /// Updates the ability slot each frame, showing cooldowns and ability status.
    /// </summary>
    private void Update()
    {
        if (GameManager.player == null || ability == null || !ability.isUnlocked)
        {
            ResetAbilitySlot();
        }
        else
        {
            UpdateAbilitySlot();
        }
    }

    /// <summary>
    /// Shows the tooltip for the ability when the pointer enters the slot.
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (ability != null && ability.isUnlocked)
        {
            GameManager.ui.TooltipWindow.Show(ability);
        }
    }

    /// <summary>
    /// Hides the tooltip when the pointer exits the slot.
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        GameManager.ui.TooltipWindow.Hide();
    }

    /// <summary>
    /// Resets the ability slot to its default state when no ability is present or unlocked.
    /// </summary>
    private void ResetAbilitySlot()
    {
        cooldownText.text = string.Empty;
        cooldownSweeper.fillAmount = 0;
        abilityIcon.enabled = false;
        reminderFlash.gameObject.SetActive(false);
    }

    /// <summary>
    /// Updates the ability slot with cooldown, availability, and visual state.
    /// </summary>
    private void UpdateAbilitySlot()
    {
        reminderFlash.gameObject.SetActive(ability.needsReminderFlash);
        abilityIcon.enabled = true;
        abilityIcon.sprite = ability.abilityIcon;

        float remainingCooldown = ability.GetRemainingCooldown(GameManager.player);
        float totalCooldown = ability.GetTotalCooldown(GameManager.player);

        abilityIcon.color = ability.HasResources(GameManager.player) ? Color.white : new Color(0.5f, 0.1f, 0.1f);
        cooldownSweeper.fillAmount = remainingCooldown / totalCooldown;
        cooldownText.text = remainingCooldown > 0 ? Mathf.Ceil(remainingCooldown).ToString() : string.Empty;
    }

    /// <summary>
    /// Formats the hotkey text for display, replacing certain key names with icons or abbreviations.
    /// </summary>
    private string FormatHotkeyText(string hotkey)
    {
        return hotkey.Replace("Alpha", "")
                     .Replace("Mouse0", "<size=120%><sprite=0 tint=1>")
                     .Replace("Mouse1", "<size=120%><sprite=1 tint=1>")
                     .Replace("Space", "S");
    }
}