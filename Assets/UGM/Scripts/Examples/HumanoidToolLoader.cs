using System.Collections;
using UnityEngine;

public class HumanoidToolLoader : UGMDownloader
{
    public HumanBodyBones humanoidBone;
    [SerializeField]
    public Vector3 positionOffset = new Vector3(0.03f, 0.08f, 0.04f);
    [SerializeField]
    private Vector3 rotationOffset = new Vector3(0, 0, -90);

    private Animator anim;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }
    protected override void OnModelFailure()
    {
        base.OnModelFailure();
    }
    protected override void OnModelSuccess(GameObject toolGO)
    {
        base.OnModelSuccess(toolGO);
        Transform parent = GetHumanoidBone(humanoidBone);
        InstantiatedGO.transform.SetParent(parent);
        InstantiatedGO.transform.localPosition = positionOffset;
        InstantiatedGO.transform.localRotation = Quaternion.Euler(rotationOffset);
        //Fix for hand rotation and position, comment or customize if not needed
        if(anim && humanoidBone == HumanBodyBones.LeftHand)
        {
            anim.SetInteger("LeftItem", 0);
        }
        if (anim && humanoidBone == HumanBodyBones.RightHand)
        {
            anim.SetInteger("RightItem", 0);
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
    }
}
