using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the player's character selection at the start of the game.
/// </summary>
public class CharacterSelectManager : MonoBehaviour
{
    // Store character strings for the characters, corresponding to their names in the main game.
    public static string selectedCharacter = "";
    public static string hoveredCharacter = "";

    private bool hasConfirmedCharacter; // Tracks if the player has confirmed their character choice.
    private AsyncOperation loadSceneOperation; // Asynchronous operation to preload the next scene.

    [SerializeField] private GameObject selectACharacter; // UI prompt for selecting a character.
    [SerializeField] private GameObject confirmButton; // UI button to confirm the selected character.
    [SerializeField] private CanvasGroup fadeOut; // CanvasGroup for fade-in/fade-out effect.

    /// <summary>
    /// Start with the screen faded out to allow a fade-in effect.
    /// </summary>
    private void OnEnable()
    {
        fadeOut.alpha = 1.0f;
    }

    /// <summary>
    /// Asynchronously loads the next scene at the start, without activating it.
    /// </summary>
    private void Start()
    {
        loadSceneOperation = SceneManager.LoadSceneAsync(
            SceneManager.GetActiveScene().buildIndex + 1, LoadSceneMode.Single);
        loadSceneOperation.allowSceneActivation = false;
    }

    /// <summary>
    /// Handles frame-specific logic for the character selection screen.
    /// </summary>
    private void Update()
    {
        // Show the prompt if no character is selected.
        selectACharacter.SetActive(string.IsNullOrEmpty(selectedCharacter));

        // Show the confirm button only if a character is selected and not confirmed yet.
        confirmButton.SetActive(!string.IsNullOrEmpty(selectedCharacter) && !hasConfirmedCharacter);

        // Fade in the scene over time if the character hasn't been confirmed.
        if (!hasConfirmedCharacter)
        {
            fadeOut.alpha -= Time.deltaTime;
        }
    }

    /// <summary>
    /// Sets the selected character.
    /// </summary>
    public void SetSelectedCharacter(string characterName)
    {
        selectedCharacter = characterName;
    }

    /// <summary>
    /// Handles the smooth loading of the game with a fade-in/fade-out effect.
    /// </summary>
    private IEnumerator LoadGame()
    {
        float fadeTime = 1.0f;
        float fadeStart = Time.time;
        float fadeEnd = fadeStart + fadeTime;

        while (Time.time < fadeEnd)
        {
            fadeOut.alpha = (Time.time - fadeStart) / fadeTime;
            yield return null;
        }

        fadeOut.alpha = 1.0f;
        yield return new WaitForSeconds(0.5f);
        loadSceneOperation.allowSceneActivation = true;
    }

    /// <summary>
    /// Handles the confirmation of the selected character and starts the game.
    /// </summary>
    public void OnConfirmPressed()
    {
        hasConfirmedCharacter = true;
        confirmButton.SetActive(false);
        StartCoroutine(LoadGame());
    }

    /// <summary>
    /// Handles the opening of the options menu.
    /// </summary>
    public void OnOptionsPressed()
    {
        // Implementation for opening options menu
    }

    /// <summary>
    /// Handles quitting the game.
    /// </summary>
    public void OnQuitPressed()
    {
        Application.Quit();
    }
}