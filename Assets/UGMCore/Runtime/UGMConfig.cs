using UnityEngine;

namespace UGM.Core
{
    [CreateAssetMenu(fileName = "UGA-Config", menuName = "ScriptableObjects/UgaConfig", order = 1)]
    public class UGMConfig : ScriptableObject
    {
        public string apiKey;
    }
}
