using PlasticGui.WebApi.Responses;
using System;
using System.Collections;
using System.IO;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class PackageDownloader
{
    [MenuItem("IGB190/Check For Content Updates")]
    private static void CheckForNewVersionMenuItem() => CheckForNewVersion(false);

    private static void CheckForNewVersion(bool silent = true)
    {
        string packageUrl = "https://igb190.github.io/updates/version.txt";
        EditorCoroutineUtility.StartCoroutineOwnerless(DownloadAndImportIfNewerVersion(packageUrl, silent));
    }

    private const string PrefLastCheckUtc = "IGB190_UpdateTxt_LastCheckUtc";
    private const double AutoCheckIntervalHours = 24;

    [InitializeOnLoadMethod]
    private static void OnLoad()
    {
        // Throttle checks
        if (!DateTime.TryParse(EditorPrefs.GetString(PrefLastCheckUtc, ""), out var last))
            last = DateTime.MinValue;

        if ((DateTime.UtcNow - last).TotalHours >= AutoCheckIntervalHours)
        {
            EditorPrefs.SetString(PrefLastCheckUtc, DateTime.UtcNow.ToString("o"));
            CheckForNewVersion(true); 
        }
    }

    private static IEnumerator DownloadAndImportIfNewerVersion(string url, bool silent)
    {
        string localVersion = GetLocalVersion();

        using (UnityWebRequest req = UnityWebRequest.Get(url))
        {
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                yield break;
            }
            string text = req.downloadHandler.text;

            string remoteVersion = GetVersionFromFile(text);

            if (float.TryParse(localVersion, out var localValue) && float.TryParse(remoteVersion, out var remoteValue))
            {
                if (localValue != remoteValue)
                {
                    UpdateApplicationEditorWindow.ShowWindow(text);
                }
                else
                {
                    Debug.Log("You are already on the latest version of the project.");
                }
            } 
        }
    }

    private static string GetLocalVersion () 
    {
        string ResourcesVersionAssetName = "version";
        var fileContents = Resources.Load<TextAsset>(ResourcesVersionAssetName);
        string text = GetVersionFromFile(fileContents.text);
        return text;
    }

    private static string GetVersionFromFile(string fileContents)
    {
        return (fileContents.Split('\n')[0]);
    }
}
