using System.Collections;
using UnityEngine;

/// <summary>
/// A script that spawns and destroys a number of AssetReferences after a given delay.
/// </summary>
public class AssetBundleSpawner : AssetBundleDownloader
{
    private GameObject instantiatedObject;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    protected override IEnumerator SpawnAsync()
    {
        yield return base.SpawnAsync();
        yield return GetAssetBundle();
        if (ab == null)
        {
            FailureEvent.Invoke(error);
            yield break;
        }
        AssetBundleRequest abr = ab.LoadAllAssetsAsync<GameObject>();
        yield return abr;
        if (abr.isDone == false)
        {
            FailureEvent.Invoke("There asset bundle failed to load");
        }
        else
        {
            instantiatedObject = Instantiate((GameObject)abr.asset, transform);
            SuccessEvent.Invoke(instantiatedObject);
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (instantiatedObject != null) Destroy(instantiatedObject);
    }
}
