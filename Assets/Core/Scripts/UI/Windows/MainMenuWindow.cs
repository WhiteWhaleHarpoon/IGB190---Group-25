using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles the logic for the main menu window, including navigation to other menus and game actions.
/// </summary>
public class MainMenuWindow : UIWindow, IPausing, ICloseable
{
    /// <summary>
    /// Opens the options menu and hides the main menu.
    /// </summary>
    public void OptionsMenuPressed()
    {
        Hide();
        GameManager.ui.OptionsWindow.Show();
    }

    /// <summary>
    /// Restarts the current level.
    /// </summary>
    public void RestartLevelPressed()
    {
        Hide();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Loads the main menu scene.
    /// </summary>
    public void MainMenuPressed()
    {
        Hide();
        SceneManager.LoadScene(0);
    }

    /// <summary>
    /// Exits the game application.
    /// </summary>
    public void ExitGamePressed()
    {
        Hide();
        Application.Quit();
    }
}