using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MyUtilities;

/// <summary>
/// Handles the display of player details, including health, resource values, abilities,
/// level, experience, gold, and shop data.
/// </summary>
public class PlayerWindow : UIWindow
{
    [SerializeField] private RectTransform abilityContainer;
    [SerializeField] private TextMeshProUGUI playerLevelText;  
    [SerializeField] private TextMeshProUGUI playerHealthText; 
    [SerializeField] private TextMeshProUGUI playerResourceText; 
    [SerializeField] private TextMeshProUGUI playerResourceNameText; 
    [SerializeField] private TextMeshProUGUI playerGoldText;   
    [SerializeField] private Image xpBar;                      
    [SerializeField] private Material healthGlobeMaterial;     
    [SerializeField] private Material resourceGlobeMaterial;   
    [SerializeField] private Image healthBar;                  
    [SerializeField] private Image resourceBar;                

    /// <summary>
    /// Performs the initial setup of the player window.
    /// </summary>
    private void Start()
    {
        SetupAbilitySlots();
        InitializePlayerStats();
        GameManager.events.OnGoldRemoved.AddListener(x => ImmediatelyUpdateGoldDisplay());
    }

    public void RedrawChacterHUD ()
    {
        for (int i = abilityContainer.transform.childCount - 1; i > 0; i--)
        {
            Destroy(abilityContainer.transform.GetChild(i).gameObject);
        }
        GameObject template = abilityContainer.GetChild(0).gameObject;
        foreach (var ability in GameManager.player.abilities)
        {
            AbilitySlotUI slot = Instantiate(template, abilityContainer).GetComponent<AbilitySlotUI>();
            slot.Setup(ability, GameManager.settings.keybindings[GameManager.player.abilities.IndexOf(ability)].ToString());
            slot.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Initializes ability slots in the player window.
    /// </summary>
    private void SetupAbilitySlots()
    {
        GameObject template = abilityContainer.GetChild(0).gameObject;
        foreach (var ability in GameManager.player.abilities)
        {
            AbilitySlotUI slot = Instantiate(template, abilityContainer).GetComponent<AbilitySlotUI>();
            slot.Setup(ability, GameManager.settings.keybindings[GameManager.player.abilities.IndexOf(ability)].ToString());
            slot.gameObject.SetActive(true);
        }
        Update();
    }

    /// <summary>
    /// Initializes player stats and resource-related elements in the UI.
    /// </summary>
    private void InitializePlayerStats()
    {
        playerResourceNameText.text = GameManager.player.resourceName;
        playerGoldText.text = ((int)GameManager.player.currentGold).ToString();
        healthGlobeMaterial = new Material(healthGlobeMaterial);
        resourceGlobeMaterial = new Material(GameManager.player.resourceMaterial);
        healthBar.material = healthGlobeMaterial;
        resourceBar.material = resourceGlobeMaterial;
    }

    /// <summary>
    /// Updates the player HUD each frame, including health, resources, XP, and gold.
    /// </summary>
    protected override void Update()
    {
        base.Update();
        UpdatePlayerStats();
        UpdateGoldDisplay();
    }

    /// <summary>
    /// Updates player stats display in the HUD.
    /// </summary>
    private void UpdatePlayerStats()
    {
        playerLevelText.text = $"{GameManager.player.currentLevel}";
        playerHealthText.text = $"{Mathf.Round(GameManager.player.health)} / " +
            $"{Mathf.Round(GameManager.player.stats.GetValue(Stat.MaxHealth))}";
        playerResourceText.text = $"{Mathf.Round(GameManager.player.resource)} / " +
            $"{Mathf.Round(GameManager.player.stats.GetValue(Stat.MaxResource))}";
        xpBar.fillAmount = GameManager.player.currentExperience / GameManager.player.experienceToNextLevel;

        SetGlobePercentage(healthGlobeMaterial, GameManager.player.health / 
            GameManager.player.stats.GetValue(Stat.MaxHealth));
        SetGlobePercentage(resourceGlobeMaterial, GameManager.player.resource / 
            GameManager.player.stats.GetValue(Stat.MaxResource));
    }

    private void ImmediatelyUpdateGoldDisplay()
    {
        playerGoldText.text = $"{GameManager.player.currentGold}";
    }

    /// <summary>
    /// Updates the displayed gold amount smoothly.
    /// </summary>
    private void UpdateGoldDisplay()
    {
        int currentDisplayedGold = int.Parse(playerGoldText.text);
        int goldDifference = Mathf.RoundToInt(GameManager.player.currentGold - currentDisplayedGold);
        goldDifference = Mathf.RoundToInt(Mathf.Min(goldDifference, 100 * Time.unscaledDeltaTime));

        int newDisplay = currentDisplayedGold + goldDifference;
        if (currentDisplayedGold > GameManager.player.currentGold)
            newDisplay = (int)GameManager.player.currentGold;

        playerGoldText.text = $"{newDisplay}";
    }

    /// <summary>
    /// Sets the fill percentage for the player stat globes (e.g., health and resource globes).
    /// </summary>
    private void SetGlobePercentage(Material globeMaterial, float percent)
    {
        globeMaterial.SetFloat("_Fill", percent);
    }
}