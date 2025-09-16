using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for all UI windows, providing common functionality for showing, hiding,
/// and managing window states such as pausing the game and handling input.
/// </summary>
public class UIWindow : MonoBehaviour
{
    // Track windows that pause the game and windows that are currently open
    public static HashSet<IPausing> pauseWindows = new HashSet<IPausing>();
    public static Stack<ICloseable> openWindows = new Stack<ICloseable>();

    private static UIWindow escapeWindow;  // The window to open when the Escape key is pressed
    private static bool handledInput = false; // Tracks whether input has been handled this frame

    /// <summary>
    /// Shows the window and manages pausing or stacking if applicable.
    /// </summary>
    public virtual void Show()
    {
        gameObject.SetActive(true);

        if (this is IPausing pausingWindow)
        {
            pauseWindows.Add(pausingWindow);
            Time.timeScale = 0.0f;
        }

        if (this is ICloseable closeableWindow && !openWindows.Contains(closeableWindow))
        {
            openWindows.Push(closeableWindow);
        }
    }

    /// <summary>
    /// Hides the window and manages unpausing or removing from the stack if applicable.
    /// </summary>
    public virtual void Hide()
    {
        gameObject.SetActive(false);

        if (this is ICloseable closeableWindow && openWindows.Count > 0 && closeableWindow == openWindows.Peek())
        {
            openWindows.Pop();
        }

        if (this is IPausing pausingWindow)
        {
            pauseWindows.Remove(pausingWindow);
            if (pauseWindows.Count == 0)
            {
                Time.timeScale = 1.0f;
            }
        }
    }

    /// <summary>
    /// Handles input, specifically checking for the Escape key to show or hide the escape window.
    /// </summary>
    protected virtual void Update()
    {
        if (!handledInput && Input.GetKeyDown(KeyCode.Escape) && Time.timeSinceLevelLoad > 1.0f)
        {
            if (openWindows.Count == 0)
            {
                escapeWindow.Show();
            }
            else if (this == (Object)openWindows.Peek())
            {
                openWindows.Pop().Hide();
                handledInput = true;
            }
        }
    }

    /// <summary>
    /// Resets input handling at the end of each frame.
    /// </summary>
    protected virtual void LateUpdate()
    {
        handledInput = false;
    }

    /// <summary>
    /// Method for setting up the window; can be overridden by derived classes.
    /// </summary>
    public virtual void Setup()
    {
        // Intentionally left blank; override in derived classes if needed
    }

    /// <summary>
    /// Sets the window that should be shown when the Escape key is pressed.
    /// </summary>
    public static void SetEscapeWindow(UIWindow window)
    {
        escapeWindow = window;
    }
}