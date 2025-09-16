using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LogicContainer))]
public class LogicContainerEditorStub : Editor
{
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Edit Logic"))
        {
            GeneralScriptEditor window = GetExistingWindow();
            if (window == null)
            {
                OpenMultipleWindows.OpenAll();
                window = GetExistingWindow();
            }
            window.SetSelectedLogicBlock((LogicContainer)target);
            window.Focus();
        }
    }

    public static GeneralScriptEditor GetExistingWindow()
    {
        GeneralScriptEditor[] windows = Resources.FindObjectsOfTypeAll<GeneralScriptEditor>();
        return windows.Length > 0 ? windows[0] : null;
    }
}
