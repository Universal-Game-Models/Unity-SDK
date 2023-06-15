using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using static UGM.Core.UGMDataTypes;

namespace UGM.Core
{
    public static class UGMManager
    {
        //A dictionary of the currently downloaded asset bundles to maintain a runtime cache
        public static Dictionary<string, AssetBundle> assetBundles = new Dictionary<string, AssetBundle>();

        //The base URI used for downloading
        public const string MODEL_URI = "https://assets.unitygameasset.com/models/";
        public const string METADATA_URI = "https://assets.unitygameasset.com/metadata/";
        public const string MODELS_OWNED_URI = "https://assets.unitygameasset.com/models-owned";

        private static UGMConfig ugmConfig = null;  
    
        /// <summary>
        /// Retrieves the UGMConfig instance, which contains the UGM configuration settings.
        /// </summary>
        /// <returns>The UGMConfig instance.</returns>
        public static UGMConfig GetConfig()
        {
            if(ugmConfig == null)
            {
                ugmConfig = Resources.Load<UGMConfig>("UGM-Config");
            }
            return ugmConfig;
        }
    
        /// <summary>
        /// Retrieves the NFTs owned by a specified address, Gets all models owned by an address, maximum 100 results, with optional pagination using a cursor.
        ///
        /// <para />
        /// If a cursor is in the response it can be used to get the next page of results
        /// </summary>
        /// <param name="address">The address for which to retrieve the NFTs owned.</param>
        /// <param name="cursor">Optional cursor for pagination.</param>
        /// <returns>A task that represents the asynchronous operation, returning an instance of the NFTsOwnedResult class.</returns>
        public static async Task<NFTsOwnedResult> GetNftsOwned(string address, string cursor = "")
        {
            // Build the full URI with the address parameter
            string fullUri = $"{MODELS_OWNED_URI}?address={address}";

            // Append the cursor parameter to the URI if it's provided
            if (!string.IsNullOrEmpty(cursor))
            {
                fullUri += $"&cursor={cursor}";
            }

            // Create a UnityWebRequest instance with the full URI
            var request = UnityWebRequest.Get(fullUri);

            // Set the "x-api-key" header using the configured API key
            request.SetRequestHeader("x-api-key", GetConfig().apiKey);

            // Create a task completion source to track the completion of the request
            var tcs = new TaskCompletionSource<bool>();

            // Send the web request asynchronously
            var operation = request.SendWebRequest();

            // Attach a callback to the completion event of the web request
            operation.completed += (asyncOperation) =>
            {
                // Set the result of the task completion source to true
                tcs.SetResult(true);
            };

            // Await the completion of the task completion source's task
            await tcs.Task;

            // Check the result of the web request
            if (request.result == UnityWebRequest.Result.Success)
            {
                // Retrieve the response data as a JSON string
                var jsonString = request.downloadHandler.text;

                try
                {
                    // Deserialize the JSON string into an instance of the NFTsOwnedResult class
                    return JsonConvert.DeserializeObject<NFTsOwnedResult>(jsonString);
                }
                catch (Exception e)
                {
                    // Handle any deserialization errors and return null
                    return null;
                }
            }
            else
            {
                // Throw an exception with the HTTP error code if the web request was not successful
                throw new Exception($"HTTP error {request.responseCode}");
            }
        }
    
        /// <summary>
        /// Clears the cache folder named "UGM" by deleting all files within it.
        /// </summary>
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
        /// <summary>
        /// Clears the cache folder named "UGM" by removing files that were last accessed before a specified cutoff date.
        /// <para />
        /// This code can be useful in scenarios where you want to manage the cache of files and remove outdated or unused
        /// data to free up storage space or ensure that only relevant files are present in the cache.
        /// </summary>
        /// <param name="cutoffDate">The cutoff date to determine which files to delete from the cache.</param>
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
}
