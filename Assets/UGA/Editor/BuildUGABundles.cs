using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class BuildUGABundles : Editor
{
    [MenuItem("Assets/Create UGA Bundle", false, 0)]
    private static void CreateBundlesFromPrefab()
    {
        // Get the selected Prefab
        GameObject prefab = Selection.activeGameObject;

        if (prefab == null)
        {
            Debug.LogError("Please select a Prefab first.");
            return;
        }

        // Set the output path for the asset bundles
        string outputPath = "assetbundles/";

        // Clear the asset bundles folder
        DirectoryInfo di = new DirectoryInfo(outputPath);
        foreach (FileInfo file in di.GetFiles())
        {
            file.Delete();
        }
        foreach (DirectoryInfo dir in di.GetDirectories())
        {
            dir.Delete(true);
        }

        // Create a subfolder for each platform
        string[] platforms = { "windows", "webgl" };
        foreach (string platform in platforms)
        {
            string folderPath = outputPath + platform;
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            string bundleName = prefab.name.ToLower();

            // Build the asset bundle for this platform
            AssetBundleBuild assetBundleBuild = new AssetBundleBuild();
            assetBundleBuild.assetBundleName = bundleName;
            assetBundleBuild.assetNames = new string[] { AssetDatabase.GetAssetPath(prefab) };
            var buildTarget = GetBuildTarget(platform);
            //Standard LZMA compression
            var bundleOptions = BuildAssetBundleOptions.None;
            if (buildTarget == BuildTarget.WebGL)
            {
                //No compression
                bundleOptions = BuildAssetBundleOptions.UncompressedAssetBundle;
                var graphicsApis = new GraphicsDeviceType[] { GraphicsDeviceType.OpenGLES3 };
                PlayerSettings.SetGraphicsAPIs(BuildTarget.WebGL, graphicsApis);
            }

            //Strip the unity version for wider client support
            bundleOptions |= BuildAssetBundleOptions.AssetBundleStripUnityVersion;
            BuildPipeline.BuildAssetBundles(folderPath, new AssetBundleBuild[] { assetBundleBuild }, bundleOptions, buildTarget);
        }

        Debug.Log("Asset bundles built successfully.");
    }

    [MenuItem("Assets/Create UGA Bundle", true)]
    private static bool ValidateOpenPrefab()
    {
        // Only enable the menu item if a Prefab is selected
        return Selection.activeGameObject != null && PrefabUtility.GetPrefabAssetType(Selection.activeGameObject) == PrefabAssetType.Regular;
    }

    private static BuildTarget GetBuildTarget(string platform)
    {
        switch (platform)
        {
            case "webgl":
                return BuildTarget.WebGL;
            case "windows":
                return BuildTarget.StandaloneWindows;
            default:
                return BuildTarget.NoTarget;
        }
    }
}
