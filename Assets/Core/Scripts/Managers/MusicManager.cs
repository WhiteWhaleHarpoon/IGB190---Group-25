using System.Collections;
using UnityEngine;

/// <summary>
/// Manages all music and sound effects in the experience.
/// </summary>
public class MusicManager : MonoBehaviour
{
    [SerializeField] private AudioSource source;       // The audio source used to control the music.
    [SerializeField] private AudioSource oneShotSource; // The audio source used to play sound effects.

    private float baseVolume = 0.2f;  // Controls base music volume - 1.0f is usually too high for background music.

    private static MusicManager instance; // Singleton reference

    /// <summary>
    /// Sets up the singleton instance and initializes the music volume.
    /// </summary>
    private void OnEnable()
    {
        if (instance == null)
        {
            instance = this;
            //baseVolume = source.volume;
            SetVolume(GameManager.settings.musicVolume);
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Plays the specified audio clip at the specified volume.
    /// </summary>
    public void PlaySound(AudioClip clip, float volume = 1.0f)
    {
        oneShotSource.PlayOneShot(clip, volume);
    }

    /// <summary>
    /// Sets the music volume to the specified amount (between 0 and 1).
    /// </summary>
    public void SetVolume(float volume)
    {
        source.volume = baseVolume * volume;
    }

    /// <summary>
    /// Fades the music out and into a new music clip.
    /// </summary>
    public void FadeIntoNewClip(AudioClip clip)
    {
        StartCoroutine(FadeToNewClip(clip, 2.0f, 2.0f, 0.0f));
    }

    /// <summary>
    /// Coroutine to handle fading between audio clips.
    /// </summary>
    private IEnumerator FadeToNewClip(AudioClip clip, float fadeOutTime = 2.0f,
                                      float fadeInTime = 2.0f, float silence = 0.0f)
    {
        float startVolume = source.volume;

        if (source.clip == null)
        {
            source.clip = clip;
        }
        else
        {
            // Handle the fade out
            yield return FadeOut(fadeOutTime, startVolume);

            // Swap the clip during silence
            source.volume = 0;
            yield return new WaitForSeconds(silence);
            source.clip = clip;
            source.Play();

            // Handle the fade in
            yield return FadeIn(fadeInTime, startVolume);
        }
    }

    /// <summary>
    /// Coroutine to fade out the current audio clip.
    /// </summary>
    private IEnumerator FadeOut(float fadeOutTime, float startVolume)
    {
        float startTime = Time.time;
        float endTime = startTime + fadeOutTime;

        while (Time.time < endTime)
        {
            source.volume = (1 - (Time.time - startTime) / fadeOutTime) * startVolume;
            yield return null;
        }
    }

    /// <summary>
    /// Coroutine to fade in the new audio clip.
    /// </summary>
    private IEnumerator FadeIn(float fadeInTime, float startVolume)
    {
        float startTime = Time.time;
        float endTime = startTime + fadeInTime;

        while (Time.time < endTime)
        {
            source.volume = ((Time.time - startTime) / fadeInTime) * startVolume;
            yield return null;
        }
    }
}