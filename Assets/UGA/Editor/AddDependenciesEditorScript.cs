using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

public class AddDependenciesEditorScript
{
    private const string manifestPath = "Packages/manifest.json";

    // Dictionary to hold the dependencies and their URLs
    private static Dictionary<string, string> dependencies = new Dictionary<string, string>();

    [MenuItem("UGM/Add Dependencies")][InitializeOnLoadMethod]
    private static void AddDeps()
    {
        var dependenciesSO = LoadDependenciesSO();
        if (dependenciesSO == null)
        {
            Debug.LogError("DependenciesSO asset not found!");
            return;
        }
        dependencies = dependenciesSO.GetDependenciesDictionary();
        EditorCoroutineUtility.StartCoroutineOwnerless(AddDependencies());
    }

    private static IEnumerator AddDependencies()
    {
        foreach (var dependency in dependencies)
        {
            if (!DependencyExists(dependency.Key, dependency.Value))
            {
                var request = Client.Add(dependency.Value);
                yield return new WaitUntil(() => request.IsCompleted);

                if (request.Status == StatusCode.Failure)
                {
                    Debug.LogError("Failed to add " + dependency.Key + " to manifest.json: " + request.Error.message);
                }
                else
                {
                    Debug.Log(dependency.Key + " added to manifest.json");
                }
            }
        }
        Client.Resolve();
    }
    private static bool DependencyExists(string packageName, string gitUrl)
    {
        var installedPackages = UnityEditor.PackageManager.PackageInfo.GetAllRegisteredPackages();

        foreach (var package in installedPackages)
        {
            if (package.name == packageName && package.packageId.Split('@')[1] == gitUrl)
            {
                return true;
            }
        }

        return false;
    }

    private static DependenciesSO LoadDependenciesSO()
    {
 
        string assetPath = "Assets/UGA/Editor/Dependencies.asset";
        return AssetDatabase.LoadAssetAtPath<DependenciesSO>(assetPath);
    }

    [System.Serializable]
    private class Manifest
    {
        public Dictionary<string, string> dependencies;
    }
}
