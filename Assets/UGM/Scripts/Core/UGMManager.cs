using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using static UGMDataTypes;

public static class UGMManager
{
    //A dictionary of the currently downloaded asset bundles to maintain a runtime cache
    public static Dictionary<string, AssetBundle> assetBundles = new Dictionary<string, AssetBundle>();

    //The base URI used for downloading
    public const string MODEL_URI = "https://assets.unitygameasset.com/models/";
    public const string METADATA_URI = "https://assets.unitygameasset.com/metadata/";
    public const string MODELS_OWNED_URI = "https://assets.unitygameasset.com/models-owned";

    private static UGMConfig ugmConfig = null;  

    public static UGMConfig GetConfig()
    {
        if(ugmConfig == null)
        {
            ugmConfig = Resources.Load<UGMConfig>("UGM-Config");
        }
        return ugmConfig;
    }

    //Gets all models owned by an address, maximum 100 results
    //If a cursor is in the response it can be used to get the next page of results
    public static async Task<NFTsOwnedResult> GetNftsOwned(string address, string cursor = "")
    {
        string fullUri = $"{MODELS_OWNED_URI}?address={address}";

        if (!string.IsNullOrEmpty(cursor))
        {
            fullUri += $"&cursor={cursor}";
        }
        var request = UnityWebRequest.Get(fullUri);
        request.SetRequestHeader("x-api-key", GetConfig().apiKey);

        var tcs = new TaskCompletionSource<bool>();
        var operation = request.SendWebRequest();

        operation.completed += (asyncOperation) =>
        {
            tcs.SetResult(true);
        };

        await tcs.Task;

        if (request.result == UnityWebRequest.Result.Success)
        {
            var jsonString = request.downloadHandler.text;
            try
            {
                return JsonConvert.DeserializeObject<NFTsOwnedResult>(jsonString);
            }
            catch (Exception e)
            {
                return null;
            }
        }
        else
        {
            throw new Exception($"HTTP error {request.responseCode}");
        }
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
    
}
