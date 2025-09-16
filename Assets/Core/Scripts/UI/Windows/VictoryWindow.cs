using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles the logic for displaying the victory window when the game is won.
/// </summary>
public class VictoryWindow : UIWindow
{
    /// <summary>
    /// Sets up the victory window, subscribing to the game won event.
    /// </summary>
    public override void Setup()
    {
        base.Setup();
        GameManager.events.OnGameWon.AddListener(() => Invoke(nameof(Show), 2.0f));
    }
}