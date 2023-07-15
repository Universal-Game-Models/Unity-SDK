using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UGM.Examples.ThirdPersonController
{
    public static class AvatarCreator
    {
        // For some reason the avatar created with this tool does not have working hands
        // The fingers seem to be disconnected from the rig and won't animate even though those bones are found and set
        public static Avatar CreateAvatar(GameObject root)
        {
            var boneMapping = new Dictionary<string, string>
            {
                {"Hips", "Hips"},
                {"Spine", "Spine"},
                {"Chest", "Spine1"},
                {"UpperChest", "Spine2"},

                {"LeftShoulder", "LeftShoulder"},
                {"LeftUpperArm", "LeftArm"},
                {"LeftLowerArm", "LeftForeArm"},
                {"LeftHand", "LeftHand"},

                {"Left Thumb Proximal", "LeftHandThumb1"},
                {"Left Thumb Intermediate","LeftHandThumb2"},
                {"Left Thumb Distal","LeftHandThumb3" },
                {"Left Index Proximal", "LeftHandIndex1"},
                {"Left Index Intermediate","LeftHandIndex2"},
                {"Left Index Distal","LeftHandIndex3" },
                {"Left Middle Proximal", "LeftHandMiddle1"},
                {"Left Middle Intermediate","LeftHandMiddle2"},
                {"Left Middle Distal","LeftHandMiddle3" },
                {"Left Ring Proximal", "LeftHandRing1"},
                {"Left Ring Intermediate","LeftHandRing2"},
                {"Left Ring Distal","LeftHandRing3" },                              
                {"Left Little Proximal", "LeftHandPinky1"},
                {"Left Little Intermediate","LeftHandPinky2"},
                {"Left Little Distal","LeftHandPinky3" },

                {"Right Thumb Proximal", "RightHandThumb1"},
                {"Right Thumb Intermediate","RightHandThumb2"},
                {"Right Thumb Distal","RightHandThumb3" },
                {"Right Index Proximal", "RightHandIndex1"},
                {"Right Index Intermediate","RightHandIndex2"},
                {"Right Index Distal","RightHandIndex3" },
                {"Right Middle Proximal", "RightHandMiddle1"},
                {"Right Middle Intermediate","RightHandMiddle2"},
                {"Right Middle Distal","RightHandMiddle3" },
                {"Right Ring Proximal", "RightHandRing1"},
                {"Right Ring Intermediate","RightHandRing2"},
                {"Right Ring Distal","RightHandRing3" },
                {"Right Little Proximal", "RightHandPinky1"},
                {"Right Little Intermediate","RightHandPinky2"},
                {"Right Little Distal","RightHandPinky3" },

                {"RightShoulder", "RightShoulder"},
                {"RightUpperArm", "RightArm"},
                {"RightLowerArm", "RightForeArm"},
                {"RightHand", "RightHand"},

                {"LeftUpperLeg", "LeftUpLeg"},
                {"LeftLowerLeg", "LeftLeg"},
                {"LeftFoot", "LeftFoot"},
                {"LeftToes", "LeftToeBase"},

                {"RightUpperLeg", "RightUpLeg"},
                {"RightLowerLeg", "RightLeg"},
                {"RightFoot", "RightFoot"},
                {"RightToes", "RightToeBase"},

                {"Neck", "Neck"},
                {"Head", "Head"},
            };

            var humanDescription = new HumanDescription
            {
                human = boneMapping.Select(mapping =>
                {
                    var bone = new HumanBone { humanName = mapping.Key, boneName = mapping.Value };
                    bone.limit.useDefaultValues = true;
                    return bone;
                }).ToArray(),
            };

            // Build the avatar and return it if valid and human
            Avatar a = AvatarBuilder.BuildHumanAvatar(root, humanDescription);
            if (a.isValid && a.isHuman)
            {
                return a;
            }
            return null;
        }
    }
}
