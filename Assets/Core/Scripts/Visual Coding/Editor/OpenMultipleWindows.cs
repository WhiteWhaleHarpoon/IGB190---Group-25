using UnityEditor;
using UnityEngine;

/// <summary>
/// Simple control to open all logic engine windows together.
/// </summary>
public class OpenMultipleWindows : EditorWindow
{
    private const int width = 1500;
    private const int height = 800;

    [MenuItem("IGB190/Open Custom Windows")]
    public static void OpenAll()
    {
        // Open each window
        var window1 = GetWindow<AbilityEditor>("Ability Editor");
        float x = Screen.currentResolution.width / 2.0f - width / 2.0f;
        float y = Screen.currentResolution.height / 2.0f - height / 2.0f;
        window1.position = new UnityEngine.Rect(x, y, width, height);
        var window2 = GetWindow<ItemEditor>("Item Editor", typeof(AbilityEditor));
        var window3 = GetWindow<GeneralScriptEditor>("Gameplay Editor", typeof(ItemEditor));
        window1.Focus();
    }
}