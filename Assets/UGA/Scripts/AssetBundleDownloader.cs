using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public abstract class AssetBundleDownloader : MonoBehaviour
{
    public string key;
    public bool clearCacheOnDestroy = false;

    public UnityEvent<object> SuccessEvent = new UnityEvent<object>();
    public UnityEvent<string> FailureEvent = new UnityEvent<string>();
    protected AssetBundle ab;
    protected string error;

    private string uri;

    protected virtual void Start()
    {
        FailureEvent.AddListener(OnFailure);
        Spawn(key);
    }

    private void OnFailure(string cause)
    {
        Debug.LogWarning("Failed to download asset bundle from " + uri + " with cause: " + cause);
    }

    public void Spawn(string key)
    {
        if (!string.IsNullOrEmpty(key))
        {
            this.key = key.ToLower();
            uri = UGAAssetManager.UGA_URI + "?platform=" + UGAAssetManager.Platform() + "&fileName=" + this.key.ToLower().Replace(" ","");
            StartCoroutine(SpawnAsync());
        }
    }

    //Base should be called at the end of overriden execution to fire the event
    protected virtual IEnumerator SpawnAsync() 
    {
        yield return null;
    }

    protected IEnumerator GetAssetBundle()
    {
        if (UGAAssetManager.assetBundles.ContainsKey(uri))
        {
            SetAssetBundle(uri, UGAAssetManager.assetBundles[uri]);
        }
        else
        {
            using (UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(uri))
            {
                uwr.SetRequestHeader("x-api-key", UGAAssetManager.GetConfig().apiKey);
                yield return uwr.SendWebRequest();
                if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError(uwr.error);
                }
                else
                {
                    if (UGAAssetManager.assetBundles.ContainsKey(uri))
                    {
                        SetAssetBundle(uri, UGAAssetManager.assetBundles[uri]);
                    }
                    else
                    {
                        SetAssetBundle(uri, DownloadHandlerAssetBundle.GetContent(uwr));
                    }
                }
                error = uwr.error;
                uwr.Dispose();
            }
        }
    }

    private void SetAssetBundle(string path, AssetBundle ab)
    {
        this.ab = ab;
        if (!UGAAssetManager.assetBundles.ContainsKey(path))
        {
            UGAAssetManager.assetBundles.Add(path, ab);
        }
    }

    protected virtual void OnDestroy()
    {
        FailureEvent.RemoveAllListeners();
        if (clearCacheOnDestroy)
        {
            //Remove from asset bundle cache
            if (UGAAssetManager.assetBundles.ContainsKey(uri))
            {
                UGAAssetManager.assetBundles.Remove(uri);
            }
            //Free up the memory of this asset bundle reference
            if (ab != null)
            {
                ab.Unload(false);
            }
        }
    }
}
