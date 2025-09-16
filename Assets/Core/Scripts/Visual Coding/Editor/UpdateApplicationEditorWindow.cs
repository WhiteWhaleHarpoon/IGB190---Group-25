using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using System.IO;
using UnityEngine.Networking;
using System.Collections;
using Unity.EditorCoroutines.Editor;
using Unity.VisualScripting;

public class UpdateApplicationEditorWindow : EditorWindow
{
    private static string updateContents = "";

    private void OnDisable()
    {
        try
        {
            Close();
        }
        catch { }
    }

    public static void ShowWindow(string contents)
    {
        var window = GetWindow<UpdateApplicationEditorWindow>("Application Updater");
        window.minSize = new Vector2(700, 400);
        window.maxSize = new Vector2(700, 400);
        updateContents = contents;
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(5);
        GUILayout.Label("An update to the IGB190 project is available. It will make the following adjustments to the project:", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        // Text field
        GUI.enabled = false;
        EditorGUILayout.TextArea(updateContents, GUILayout.ExpandHeight(true));
        GUI.enabled = true;
        EditorGUILayout.Space(5);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Download Latest"))
        {
            string packageUrl = "https://igb190.github.io/updates/package.unitypackage";
            EditorCoroutineUtility.StartCoroutineOwnerless(DownloadAndImportRoutine(packageUrl));
        }
        if (GUILayout.Button("Close"))
        {
            Close();
        }

        EditorGUILayout.EndHorizontal();
    }

    private static IEnumerator DownloadAndImportRoutine(string url)
    {
        string fileName = Path.GetFileName(url);
        string tempPath = Path.Combine(Path.GetTempPath(), fileName);

        using (UnityWebRequest req = UnityWebRequest.Get(url))
        {
            req.downloadHandler = new DownloadHandlerFile(tempPath);
            var op = req.SendWebRequest();

            while (!op.isDone)
            {
                EditorUtility.DisplayProgressBar("Downloading Package",
                    $"Downloading {fileName}...", req.downloadProgress);
                yield return null;
            }
            EditorUtility.ClearProgressBar();

            if (req.result != UnityWebRequest.Result.Success)
            {
                EditorUtility.DisplayDialog("Download Failed", req.error, "OK");
                yield break;
            }
        }

        if (File.Exists(tempPath))
        {
            GetWindow<UpdateApplicationEditorWindow>("Application Updater").Close();
            AssetDatabase.ImportPackage(tempPath, true);
        }
    }
}