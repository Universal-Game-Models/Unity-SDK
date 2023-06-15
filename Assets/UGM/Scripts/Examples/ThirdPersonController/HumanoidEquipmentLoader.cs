using UGM.Core;
using UnityEngine;

namespace UGM.Examples.ThirdPersonController
{
    /// <summary>
    /// Manages the loading and positioning of humanoid equipment on the character model.
    /// Inherits from the UGMDownloader class.
    /// </summary>
    public class HumanoidEquipmentLoader : UGMDownloader
    {
        public HumanBodyBones humanoidBone;
        [SerializeField]
        public Vector3 positionOffset = new Vector3(0.03f, 0.08f, 0.04f);
        [SerializeField]
        private Vector3 rotationOffset = new Vector3(0, 0, -90);

        private Animator anim;


        public void ToggleLoad(string tokenId)
        {
            if(tokenId == this.nftId && InstantiatedGO)
            {
                this.nftId = "";
                DestroyImmediate(InstantiatedGO);
            }
            else
            {
                Load(tokenId);
            }

        }
        /// <summary>
        /// Called when the model loading succeeds.
        /// Calls the base OnModelSuccess method from the parent class.
        /// Gets the parent bone transform and sets the position and rotation of the instantiated game object.
        /// Fixes the hand rotation and position if necessary.
        /// </summary>
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

        /// <summary>
        /// Retrieves the humanoid bone transform based on the specified bone type.
        /// Searches for the bone transform in the parent or sibling objects.
        /// </summary>
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

        /// <summary>
        /// Called when the component is being destroyed.
        /// Resets the hand animation states to -1.
        /// Calls the base OnDestroy method from the parent class.
        /// </summary>
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
}
