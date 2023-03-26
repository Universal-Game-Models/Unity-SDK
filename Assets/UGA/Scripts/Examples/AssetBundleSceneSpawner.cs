using System.Collections;
using UnityEngine.SceneManagement;

internal class AssetBundleSceneSpawner : AssetBundleDownloader
{
    private Scene scene;
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
            FailureEvent.Invoke("The asset bundle was null");
            yield break;
        }
        var scenePaths = ab.GetAllScenePaths();

        var loadOp = SceneManager.LoadSceneAsync(scenePaths[0], LoadSceneMode.Additive);
        yield return loadOp;
        if (!loadOp.isDone)
        {
            FailureEvent.Invoke("The asset bundle scene failed to load");
        }
        scene = SceneManager.GetSceneByName(key);
        SuccessEvent.Invoke(scene);
    }

    protected override void OnDestroy()
    {
        if(scene != null && scene.isLoaded) SceneManager.UnloadSceneAsync(scene);
        base.OnDestroy();
    }
}