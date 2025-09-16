using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// The MessageWindow class allows messages to be displayed to the user in the center
/// of the screen for a specific period of time.
/// </summary>
public class MessageWindow : UIWindow
{
    [SerializeField] private TextMeshProUGUI message;
    [SerializeField] private CanvasGroup canvasGroup;
    private float lastMessageDisplayedAt = 0;
    private float displayTime;

    /// <summary>
    /// Handle the fade-in and fade-out of the message.
    /// </summary>
    protected override void Update()
    {
        base.Update();
        canvasGroup.alpha = Fade(Time.time - lastMessageDisplayedAt, displayTime);
        if (Time.time > lastMessageDisplayedAt + displayTime + 1.0f)
        {
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Display a message on the screen for a specific period of time.
    /// </summary>
    public void DisplayMessage(string text, float displayTime)
    {
        if (!GameManager.settings.showTutorialMessages) return;
        message.text = text;
        lastMessageDisplayedAt = Time.time;
        gameObject.SetActive(true);
        canvasGroup.alpha = 0;
        this.displayTime = displayTime;
        GameManager.assets.notificationReceivedFeedback.ActivateFeedback();
    }

    /// <summary>
    /// Calculate the current alpha based on the current time since the
    /// message was shown, and the intended display and fade times.
    /// </summary>
    public static float Fade(float t, float holdTime = 1.0f, 
        float fadeInTime = 0.5f, float fadeOutTime = 0.5f)
    {
        if (t < fadeInTime) return (t / fadeInTime);
        if (t < fadeInTime + holdTime) return 1.0f;
        else return 1 - (t - holdTime - fadeInTime) / fadeOutTime;
    }
}
