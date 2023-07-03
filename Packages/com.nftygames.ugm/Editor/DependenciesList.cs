using UnityEngine;

namespace UGM.Editor
{
    public static class DependenciesList
    {
        public static readonly DependenciesInfo[] Dependencies =
        {
            new DependenciesInfo
            {
                name = "com.atteneder.gltfast",
                gitUrl = "https://github.com/atteneder/glTFast.git#v5.0.0",
                version = "5.0.0"
            },
            
            new DependenciesInfo
            {
                name = "com.dbrizov.naughtyattributes",
                gitUrl = "https://github.com/dbrizov/NaughtyAttributes.git#upm",
                version = "2.1.4"
            },
            new DependenciesInfo
            {
                name = "com.nftygames.ugmcore",
                gitUrl = "https://github.com/Universal-Game-Models/Unity-SDK.git#upmcore",
                version = "0.0.1"
            }
        };
    }
}
