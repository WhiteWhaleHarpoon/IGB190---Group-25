using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeathWindow : UIWindow, IPausing
{
    [SerializeField] private TextMeshProUGUI causeOfDeathText;
    [SerializeField] private TextMeshProUGUI respawnTimeRemaining;
    [SerializeField] private Button respawnButton;
    private float windowShownAt;

    public float respawnDelay = 5;

    private void OnEnable()
    {
        windowShownAt = Time.unscaledTime;
    }

    protected override void Update()
    {
        base.Update();
        int secondsRemaining = Mathf.CeilToInt(respawnDelay - (Time.unscaledTime - windowShownAt));
        if (secondsRemaining > 0)
        {
            respawnTimeRemaining.text = $"Waiting to Respawn ({secondsRemaining})";
            respawnButton.interactable = false;
        }
        else
        {
            respawnTimeRemaining.text = $"Respawn";
            respawnButton.interactable = true;
        }
    }

    public override void Setup()
    {
        base.Setup();
        GameManager.events.OnPlayerKilled.AddListener(OnPlayerKilled);
    }

    /// <summary>
    /// Handles the player being killed by updating the death message.
    /// </summary>
    private void OnPlayerKilled(GameEvents.OnPlayerKilledInfo info)
    {
        // Update the cause of death text based on whether a unit caused the death
        causeOfDeathText.text = info.killingUnit != null ? $"You were slain by a {info.killingUnit.unitName}" : "You were slain";

        // Show the death window after a delay
        Invoke(nameof(Show), 2.0f);
    }

    /// <summary>
    /// Revives the player and hides the death window.
    /// </summary>
    public void RevivePlayer()
    {
        GameManager.player.Revive();
        GameManager.player.stats.Get(Stat.DamageTaken).AddTimedPercentageModifier(0, 2); // Temporary invincibility
        Hide();
    }
}