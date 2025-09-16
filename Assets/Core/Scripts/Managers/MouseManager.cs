using UnityEngine;

/// <summary>
/// Manages the cursor appearance based on player actions, such as hovering over enemies or interacting with the UI.
/// </summary>
public class MouseManager : MonoBehaviour
{
    public static MouseManager Instance { get; private set; }

    [SerializeField] private Texture2D mouseRegular;
    [SerializeField] private Texture2D mouseDown;

    /// <summary>
    /// Sets up the singleton instance of the MouseManager.
    /// </summary>
    private void OnEnable()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Updates the cursor appearance based on the player's mouse input.
    /// </summary>
    private void Update()
    {
        Cursor.SetCursor(Input.GetMouseButton(0) ? mouseDown : mouseRegular, Vector2.zero, CursorMode.Auto);
    }
}