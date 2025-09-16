using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls the sequence of story panels, including text, images, and audio playback.
/// </summary>
public class StoryController : MonoBehaviour
{
    [Header("Controller Settings")]
    [SerializeField] private float delayBeforeDialogue = 1.0f;
    [SerializeField] private float fadeInTime = 1.5f;
    [SerializeField] private float fadeOutTime = 1.5f;
    [SerializeField] private float stayFadedTime = 0.5f;
    [SerializeField] private float panTime = 12f;
    [SerializeField] private IntroItem[] introItems;

    [Header("Cached References")]
    [SerializeField] private Image image;
    [SerializeField] private Image fade;
    [SerializeField] private Image skipFill;
    [SerializeField] private GameObject skipComponent;
    [SerializeField] private TextMeshProUGUI message;
    private AudioSource source;
    private AsyncOperation asyncLoad = null;

    private float currentSkipTime = 0;
    private float lastButtonPress = -99999f;

    private const float TimeToSkip = 2f;
    private const float ShowSkipTime = 2f;

    /// <summary>
    /// Stores data for a single story item, including image, message, audio, and positions.
    /// </summary>
    [System.Serializable]
    public class IntroItem
    {
        public Sprite sprite;
        public string message;
        public AudioClip clip;
        public float time = 5f;
        public Vector2 start = Vector2.zero;
        public Vector2 end = Vector2.zero;
    }

    /// <summary>
    /// Initializes the story controller and starts showing the story items.
    /// </summary>
    private void Start()
    {
        source = GetComponent<AudioSource>();
        LoadNextSceneAsync();
        StartCoroutine(ShowItems());
    }

    /// <summary>
    /// Handles logic for skipping the story panels and updates the UI.
    /// </summary>
    private void Update()
    {
        if (Input.anyKey) lastButtonPress = Time.time;

        skipComponent.SetActive(currentSkipTime > 0 || Time.time < lastButtonPress + ShowSkipTime);

        if (Input.GetKey(KeyCode.Escape)) currentSkipTime += Time.deltaTime;
        if (Input.GetKeyUp(KeyCode.Escape)) currentSkipTime = 0;

        skipFill.fillAmount = currentSkipTime / TimeToSkip;

        if (currentSkipTime > TimeToSkip) asyncLoad.allowSceneActivation = true;
    }

    /// <summary>
    /// Displays all story items sequentially, managing the fade, pan, and text effects.
    /// </summary>
    private IEnumerator ShowItems()
    {
        foreach (IntroItem item in introItems)
        {
            message.text = string.Empty;
            image.sprite = item.sprite;

            StartCoroutine(FadeInText(item.message, delayBeforeDialogue));
            StartCoroutine(PlayAudio(item.clip, delayBeforeDialogue));
            StartCoroutine(PanImage(item.start, item.end, panTime));

            yield return StartCoroutine(FadeIn(fadeInTime));
            yield return new WaitForSeconds(item.time);
            yield return StartCoroutine(FadeOut(fadeOutTime));
            yield return new WaitForSeconds(stayFadedTime);
        }

        asyncLoad.allowSceneActivation = true;
    }

    /// <summary>
    /// Plays the specified audio clip after a delay.
    /// </summary>
    private IEnumerator PlayAudio(AudioClip clip, float audioDelay = 1.5f)
    {
        if (clip != null)
        {
            yield return new WaitForSeconds(audioDelay);
            source.clip = clip;
            source.Play();
        }
    }

    /// <summary>
    /// Asynchronously loads the next scene in the background.
    /// </summary>
    private void LoadNextSceneAsync()
    {
        int nextSceneIndex = (SceneManager.GetActiveScene().buildIndex + 1) % SceneManager.sceneCountInBuildSettings;
        asyncLoad = SceneManager.LoadSceneAsync(nextSceneIndex, LoadSceneMode.Single);
        asyncLoad.allowSceneActivation = false;
    }

    /// <summary>
    /// Fades the screen in over the specified time.
    /// </summary>
    private IEnumerator FadeIn(float fadeTime)
    {
        float startAlpha = fade.color.a;
        float elapsedTime = 0f;

        while (elapsedTime < fadeTime)
        {
            fade.color = new Color(0, 0, 0, Mathf.Lerp(startAlpha, 0f, elapsedTime / fadeTime));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        fade.color = new Color(0, 0, 0, 0);
    }

    /// <summary>
    /// Fades the screen out over the specified time.
    /// </summary>
    private IEnumerator FadeOut(float fadeTime)
    {
        float startAlpha = fade.color.a;
        float elapsedTime = 0f;

        while (elapsedTime < fadeTime)
        {
            fade.color = new Color(0, 0, 0, Mathf.Lerp(startAlpha, 1f, elapsedTime / fadeTime));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        fade.color = Color.black;
    }

    /// <summary>
    /// Pans the image from the start position to the end position over the specified time.
    /// </summary>
    private IEnumerator PanImage(Vector2 start, Vector2 end, float duration)
    {
        RectTransform rectTransform = image.GetComponent<RectTransform>();
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            rectTransform.anchoredPosition = Vector2.Lerp(start, end, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    /// <summary>
    /// Gradually fades in the text character by character.
    /// </summary>
    private IEnumerator FadeInText(string text, float startDelay = 0f, float timePerCharacter = 0.02f, float characterFadeTime = 1f)
    {
        yield return new WaitForSeconds(startDelay);
        float fadeStart = Time.time;
        float fadeEnd = fadeStart + timePerCharacter * text.Length + characterFadeTime;

        while (Time.time < fadeEnd)
        {
            string fadedText = string.Empty;
            bool addedClear = false;

            for (int i = 0; i < text.Length; i++)
            {
                Color c = message.color;
                float timeSinceCharacterFadeIn = Time.time - (fadeStart + timePerCharacter * i);

                if (timeSinceCharacterFadeIn <= 0)
                {
                    if (!addedClear)
                    {
                        fadedText += $"<color=#{ColorUtility.ToHtmlStringRGBA(Color.clear)}>";
                        addedClear = true;
                    }
                }
                else if (timeSinceCharacterFadeIn < characterFadeTime)
                {
                    c.a = timeSinceCharacterFadeIn / characterFadeTime;
                    fadedText += $"<color=#{ColorUtility.ToHtmlStringRGBA(c)}>";
                }
                fadedText += text[i];
            }

            message.text = fadedText;
            yield return null;
        }

        message.text = text;
    }
}