using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Item))]
public class ItemEditorStub : Editor
{
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Edit Item"))
        {
            ItemEditor window = GetExistingWindow();
            if (window == null)
            {
                OpenMultipleWindows.OpenAll();
                window = GetExistingWindow();
            }
            window.SetSelectedItem((Item)target);
            window.Focus();
        }
    }

    public static ItemEditor GetExistingWindow()
    {
        ItemEditor[] windows = Resources.FindObjectsOfTypeAll<ItemEditor>();
        return windows.Length > 0 ? windows[0] : null;
    }
}
