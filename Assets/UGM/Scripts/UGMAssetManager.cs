using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class UGMAssetManager
{
    //A dictionary of the currently downloaded asset bundles to maintain a runtime cache
    public static Dictionary<string, AssetBundle> assetBundles = new Dictionary<string, AssetBundle>();

    //The base URI used for downloading
    public const string MODEL_URI = "https://assets.unitygameasset.com/models/";
    public const string METADATA_URI = "https://assets.unitygameasset.com/metadata/";

    private static UGMConfig ugmConfig = null;

    //The platform string for downloading
    public static string Platform() 
    {
        switch (Application.platform)
        {
            case RuntimePlatform.OSXEditor:
            case RuntimePlatform.OSXPlayer:
                return "mac";
            case RuntimePlatform.WindowsPlayer:
            case RuntimePlatform.WindowsEditor:
                return "standalonewindows";
            case RuntimePlatform.IPhonePlayer:
                return "ios";
            case RuntimePlatform.Android:
                return "android";
            case RuntimePlatform.LinuxPlayer:
            case RuntimePlatform.LinuxEditor:
                return "linux";
            case RuntimePlatform.WebGLPlayer:
                return "webgl";
            default:
                return "";
        }
    }

    public static UGMConfig GetConfig()
    {
        if(ugmConfig == null)
        {
            ugmConfig = Resources.Load<UGMConfig>("UGM-Config");
        }
        return ugmConfig;
    }

    public static void ClearCache()
    {
        string cacheDirectory = Path.Combine(Application.persistentDataPath, "UGM");

        if (Directory.Exists(cacheDirectory))
        {
            DirectoryInfo directory = new DirectoryInfo(cacheDirectory);

            foreach (FileInfo file in directory.GetFiles())
            {
                file.Delete();
            }
        }
        Debug.Log("Cleared UGM cache folder");
    }
    public static void ClearCacheByAccessDate(DateTime cutoffDate)
    {
        string cacheDirectory = Path.Combine(Application.persistentDataPath, "UGM");

        if (Directory.Exists(cacheDirectory))
        {
            DirectoryInfo directory = new DirectoryInfo(cacheDirectory);

            foreach (FileInfo file in directory.GetFiles())
            {
                if (file.LastAccessTime < cutoffDate)
                {
                    file.Delete();
                }
            }
        }
        Debug.Log($"Cleared UGM cache folder of files last accessed before {cutoffDate}");
    }
    public class Metadata
    {
        public string name;
        public string description;
        public string image;
        public Attribute[] attributes;
    }
    public class Attribute
    {
        public string trait_type;
        public object value;
    }
}