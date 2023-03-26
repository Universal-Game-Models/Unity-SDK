using System.Collections;
using UnityEngine;

public class AssetBundleHumanoidToolSpawner : AssetBundleDownloader
{
    public HumanBodyBones humanoidBone;
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
            FailureEvent.Invoke("The asset bundle was null");
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
            Transform parent = GetHumanoidHand(humanoidBone);
            instantiatedObject = Instantiate((GameObject)abr.asset, parent);
            //Fix for right hand rotation and position, comment out if not needed
            if (humanoidBone == HumanBodyBones.RightHand)
            {
                var rot = instantiatedObject.transform.localEulerAngles;
                instantiatedObject.transform.localRotation = Quaternion.Euler(rot.x, rot.y, -rot.z);

                var pos = instantiatedObject.transform.localPosition;
                instantiatedObject.transform.localPosition = new Vector3(-pos.x, -pos.y, pos.z);
            }
            SuccessEvent.Invoke(instantiatedObject);
        }
    }

    private Transform GetHumanoidHand(HumanBodyBones bone)
    {
        var siblingCount = transform.parent.childCount;
        for (int i = 0; i < siblingCount; i++)
        {
            //Don't check yourself for the animator
            if(i != transform.GetSiblingIndex())
            {
                if(transform.parent.GetChild(i).TryGetComponent(out Animator anim))
                {
                    var parentBone = anim.GetBoneTransform(bone);
                    if (parentBone != null)
                    {
                        return (parentBone);
                    }
                    
                }
            }
        }
        Debug.LogWarning("Did not find the Humanoid Bone " + humanoidBone.ToString());
        return null;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (instantiatedObject != null) Destroy(instantiatedObject);
    }
}
