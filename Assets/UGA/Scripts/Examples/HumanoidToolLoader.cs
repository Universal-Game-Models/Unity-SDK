using System.Collections;
using UnityEngine;

public class HumanoidToolLoader : UGADownloader
{
    public HumanBodyBones humanoidBone;
    private Animator anim;
    private void Awake()
    {
        onSuccess.AddListener(OnLoadCompleted);
        onFailure.AddListener(OnLoadFailed);
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }
    public void Load(string assetName)
    {
        this.assetName = assetName;
        LoadAsset();
    }
    /*protected override IEnumerator SpawnAsync()
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
    */
    private void OnLoadFailed()
    {

    }
    private void OnLoadCompleted(GameObject toolGO)
    {
        Transform parent = GetHumanoidBone(humanoidBone);
        toolGO.transform.SetParent(parent);
        toolGO.transform.localPosition = Vector3.zero;
        toolGO.transform.localRotation = Quaternion.identity;
        //Fix for hand rotation and position, comment or customize if not needed
        if(humanoidBone == HumanBodyBones.LeftHand)
        {
            toolGO.transform.localRotation = Quaternion.Euler(0, 0, -90);
            toolGO.transform.localPosition = new Vector3(0, 0.05f, 0.04f);
            if (anim) {
                anim.SetInteger("LeftItem", 0);
            }
        }
        if (humanoidBone == HumanBodyBones.RightHand)
        {
            toolGO.transform.localRotation = Quaternion.Euler(0, 0, 90);
            toolGO.transform.localPosition = new Vector3(0, 0.05f, 0.04f);
            if (anim)
            {
                anim.SetInteger("RightItem", 0);
            }
        }
    }
    private Transform GetHumanoidBone(HumanBodyBones bone)
    {
        //Check my parent for Animator
        if (transform.parent.TryGetComponent(out anim))
        {
            var parentBone = anim.GetBoneTransform(bone);
            if (parentBone != null)
            {
                return (parentBone);
            }

        }
        var siblingCount = transform.parent.childCount;
        //Check my siblings for an Animator
        for (int i = 0; i < siblingCount; i++)
        {
            //Don't check yourself for the animator
            if(i != transform.GetSiblingIndex())
            {
                if(transform.parent.GetChild(i).TryGetComponent(out anim))
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
        if (anim)
        {
            anim.SetInteger("LeftItem", -1);
            anim.SetInteger("RightItem", -1);
        }
        base.OnDestroy();
        onSuccess.RemoveListener(OnLoadCompleted);
        onFailure.RemoveListener(OnLoadFailed);
    }
}
