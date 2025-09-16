using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// The NotificationWindow class displays messages to the user in the center
/// of the screen for a specific period of time.
/// </summary>
public class NotificationWindow : UIWindow
{
    [SerializeField] private TextMeshProUGUI title;        // UI element for the title text
    [SerializeField] private TextMeshProUGUI content;      // UI element for the content text
    [SerializeField] private GameObject iconContainer;     // Container for the icon
    [SerializeField] private Image iconImage;              // UI element for the icon image
    [SerializeField] private CanvasGroup canvasGroup;      // CanvasGroup for controlling fade effects

    private float lastMessageDisplayedAt = -9999f;         
    private readonly Queue<Notification> notificationsInQueue = new Queue<Notification>();
    private const float DISPLAY_TIME = 3.0f;              

    public override void Setup()
    {
        base.Setup();

        // Subscribe to events to display appropriate messages
        GameManager.events.OnPlayerLevelUp.AddListener(player =>
        {
            DisplayMessage("Level Up", $"You have reached level {player.currentLevel}.");
        });
        GameManager.events.OnQuestAdded.AddListener(quest =>
        {
            DisplayMessage("Quest Received", quest.Description);
        });
        GameManager.events.OnQuestCompleted.AddListener(quest =>
        {
            DisplayMessage("Quest Completed", quest.Description);
        });
    }

    /// <summary>
    /// Class representing a notification with title, content, and an optional icon.
    /// </summary>
    private class Notification
    {
        public string Title { get; }
        public string Content { get; }
        public Sprite Icon { get; }

        public Notification(string title, string content, Sprite icon)
        {
            Title = title;
            Content = content;
            Icon = icon;
        }
    }

    /// <summary>
    /// Handles the fade-in and fade-out of messages, and manages the queue of notifications.
    /// </summary>
    protected override void Update()
    {
        base.Update();

        if (Time.time > lastMessageDisplayedAt + DISPLAY_TIME)
        {
            if (notificationsInQueue.Count > 0)
            {
                // Dequeue and display the next notification
                Notification notification = notificationsInQueue.Dequeue();
                title.text = notification.Title;
                content.text = notification.Content;
                lastMessageDisplayedAt = Time.time;

                // Update icon display
                iconContainer.SetActive(notification.Icon != null);
                iconImage.sprite = notification.Icon;
            }
            else
            {
                // No more notifications, hide the window
                gameObject.SetActive(false);
            }
        }

        // Update the canvas group's alpha for fade effect
        canvasGroup.alpha = Fade(Time.time - lastMessageDisplayedAt);
    }

    /// <summary>
    /// Enqueue a message to be displayed on the screen.
    /// </summary>
    public void DisplayMessage(string title, string content, Sprite icon = null)
    {
        notificationsInQueue.Enqueue(new Notification(title, content, icon));
        canvasGroup.alpha = 0;
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Calculates the alpha value for the fade effect based on time.
    /// </summary>
    public float Fade(float t, float holdTime = 2.0f, float fadeInTime = 0.5f, float fadeOutTime = 0.5f)
    {
        if (t < fadeInTime)
            return t / fadeInTime;       
        if (t < fadeInTime + holdTime)
            return 1.0f;                 
        return 1 - (t - holdTime - fadeInTime) / fadeOutTime;
    }
}