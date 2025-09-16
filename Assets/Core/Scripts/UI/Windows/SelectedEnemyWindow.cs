using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays details about the currently selected monster in the game.
/// </summary>
public class SelectedEnemyWindow : UIWindow
{
    [SerializeField] private Transform container;             
    [SerializeField] private TextMeshProUGUI enemyName;       
    [SerializeField] private TextMeshProUGUI enemyHealth;     
    [SerializeField] private TextMeshProUGUI enemyAbilities;  
    [SerializeField] private Image healthFill;                

    /// <summary>
    /// Updates the window with the current monster's details each frame.
    /// </summary>
    protected override void Update()
    {
        base.Update();

        if (ShouldDisplayEnemyDetails())
        {
            DisplayEnemyDetails();
        }
        else
        {
            HideEnemyDetails();
        }
    }

    /// <summary>
    /// Determines if the enemy details should be displayed.
    /// </summary>
    private bool ShouldDisplayEnemyDetails()
    {
        return GameManager.hoveredMonster != null && GameManager.settings.showTopHealthBarUI;
    }

    /// <summary>
    /// Displays the details of the currently hovered monster.
    /// </summary>
    private void DisplayEnemyDetails()
    {
        container.gameObject.SetActive(true);
        enemyName.text = GameManager.hoveredMonster.unitName;
        enemyHealth.text = Mathf.Round(GameManager.hoveredMonster.health).ToString();
        healthFill.fillAmount = GameManager.hoveredMonster.health / GameManager.hoveredMonster.stats.GetValue(Stat.MaxHealth);

        enemyAbilities.text = string.Join(", ", GameManager.hoveredMonster.abilities.ConvertAll(ability => ability.abilityName));
    }

    /// <summary>
    /// Hides the enemy details when no enemy is selected.
    /// </summary>
    private void HideEnemyDetails()
    {
        container.gameObject.SetActive(false);
    }
}