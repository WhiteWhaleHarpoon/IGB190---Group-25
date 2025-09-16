using UnityEngine;
using TMPro;

/// <summary>
/// Handles all logic related to the player descriptors and equipment in the equipment window.
/// </summary>
public class EquipmentWindow : UIWindow
{
    [SerializeField] private RectTransform equipmentContainer; // Container for equipment slots
    [SerializeField] private TextMeshProUGUI characterName; // UI element for displaying the character's name
    [SerializeField] private TextMeshProUGUI characterLevel; // UI element for displaying the character's level
    private EquipmentSlot[] equipmentSlots; // Array to hold references to the equipment slots

    /// <summary>
    /// Sets up the equipment window when it is enabled.
    /// </summary>
    private void OnEnable()
    {
        // Update the character name and level in the UI
        characterName.text = GameManager.player.unitName;
        characterLevel.text = $"Level {GameManager.player.currentLevel}";

        // Initialize and setup equipment slots if not already done
        if (equipmentSlots == null)
        {
            equipmentSlots = equipmentContainer.GetComponentsInChildren<EquipmentSlot>(true);
            for (int i = 0; i < equipmentSlots.Length; i++)
            {
                equipmentSlots[i].Setup(GameManager.player.equipment, i);
            }
        }
    }
}