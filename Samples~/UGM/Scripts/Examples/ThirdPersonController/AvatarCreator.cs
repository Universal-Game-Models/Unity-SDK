using System;
using System.Collections.Generic;
using UnityEngine;

namespace UGM.Examples.ThirdPersonController
{
    public static class AvatarCreator
    {
        // For some reason the avatar created with this tool does not have working hands
        // The fingers seem to be disconnected from the rig and won't animate even though those bones are found and set
        // Otherwise this can import RPM characters and create avatars at runtime
        public static Avatar CreateAvatar(Animator anim)
        {
            var bones = new List<HumanBone>();
            var skeleton = new List<SkeletonBone>();
            // Find all the bone transforms in the children of the game object
            var allTransforms = anim.transform.GetChild(0).GetComponentsInChildren<Transform>(true);
            foreach (var childTransform in allTransforms)
            {
                var humanBoneName = GetHumanBoneName(childTransform.name);
                //Human bones
                if (humanBoneName != null)
                {
                    Debug.Log("HUMAN BONE: " + humanBoneName + " Transform: " + childTransform.name);
                    var humanBone = new HumanBone();
                    humanBone.humanName = humanBoneName;
                    humanBone.boneName = childTransform.name;
                    humanBone.limit.useDefaultValues = true;
                    bones.Add(humanBone);

                    var skeletonBone = new SkeletonBone();
                    skeletonBone.name = childTransform.name;
                    skeletonBone.position = childTransform.localPosition;
                    skeletonBone.rotation = childTransform.localRotation;
                    skeletonBone.scale = childTransform.lossyScale;
                    skeleton.Add(skeletonBone);
                }
                //Other Bones
                else
                {
                    var sb = new SkeletonBone();
                    sb.name = childTransform.name;
                    sb.position = childTransform.localPosition;
                    sb.rotation = childTransform.localRotation;
                    sb.scale = childTransform.lossyScale;
                    skeleton.Add(sb);
                }
            }

            // Create the human description from the human and skeleton bones
            var humanDescription = new HumanDescription();
            humanDescription.human = bones.ToArray();
            humanDescription.skeleton = skeleton.ToArray();
            humanDescription.armStretch = 1f;
            humanDescription.legStretch = 0.05f;
            humanDescription.upperArmTwist = 0;
            humanDescription.lowerArmTwist = 0;
            humanDescription.upperLegTwist = 0.5f;
            humanDescription.lowerLegTwist = 0.5f;
            humanDescription.feetSpacing = 0.0f;
            // Build the avatar and return it if valid and human
            Avatar a = AvatarBuilder.BuildHumanAvatar(anim.gameObject, humanDescription);
            if (a.isValid && a.isHuman)
            {
                // Save the avatar to a file to debug
                //string assetPath = "Assets/MyAvatar.asset";
                //AssetDatabase.CreateAsset(a, assetPath);
                //AssetDatabase.SaveAssets();
                //AssetDatabase.Refresh();
                return a;
            }
            return null;
        }

        private static string GetHumanBoneName(string value)
        {
            //The name matches one of the HumanBodyBones
            if (Enum.TryParse(value, out HumanBodyBones bone))
            {
                return bone.ToString();
            }

            //Workaround for ready player me models that aren't named the same as HumanBodyBones 
            if (value.Equals("LeftUpLeg") || value.Equals("RightUpLeg"))
            {
                value = value.Replace("Up", "Upper");
            }
            else if (value.Equals("LeftLeg") || value.Equals("RightLeg"))
            {
                value = value.Replace("Leg", "LowerLeg");
            }
            else if (value.Equals("LeftForeArm") || value.Equals("RightForeArm"))
            {
                value = value.Replace("Fore", "Lower");
            }
            else if (value.Equals("LeftArm") || value.Equals("RightArm"))
            {
                value = value.Replace("Arm", "UpperArm");
            }
            else if (value.Equals("Spine1"))
            {
                value = "Chest";
            }
            else if (value.Equals("Spine2"))
            {
                value = "UpperChest";
            }
            else if(value.Equals("LeftToeBase") || value.Equals("RightToeBase"))
            {
                value = value.Replace("Base", "");
            }
            else if (value.Contains("Thumb") || value.Contains("Index") || value.Contains("Middle") || value.Contains("Ring") || value.Contains("Pinky"))
            {
                value = value.Replace("Hand", "").Replace("Pinky","Little").Replace("1", "Proximal").Replace("2", "Intermediate").Replace("3", "Distal");
            }

            if (Enum.TryParse(value, out HumanBodyBones bn))
            {
                return value;
            }
            return null;
        }
    }
}
