using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Ability))]
public class AbilityEditorStub : Editor
{
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Edit Ability"))
        {
            AbilityEditor window = GetExistingWindow();
            if (window == null)
            {
                OpenMultipleWindows.OpenAll();
                window = GetExistingWindow();
            }
            window.SetSelectedAbility((Ability)target);
            window.Focus();
        }
    }

    public static AbilityEditor GetExistingWindow()
    {
        AbilityEditor[] windows = Resources.FindObjectsOfTypeAll<AbilityEditor>();
        return windows.Length > 0 ? windows[0] : null;
    }
}
