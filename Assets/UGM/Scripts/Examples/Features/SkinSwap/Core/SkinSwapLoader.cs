using System.Threading.Tasks;
using NaughtyAttributes;
using UGM.Core;
using UGM.Examples.Features.SkinSwap.Interface;
using UnityEngine;

namespace UGM.Examples.Features.SkinSwap.Core
{
    public class SkinSwapLoader : UGMDownloader, ILoadableSkin, ISwitchableSkin
    {
        [SerializeField]
        protected UGMDownloader weaponUGM;

        private GameObject weaponGameObject;
        private string defaultId;
        public void LoadSkin(UGMDataTypes.TokenInfo data)
        {
            if (weaponGameObject == null)
                weaponGameObject = weaponUGM.InstantiatedGO;
            weaponGameObject.SetActive(false);
            Load(data.token_id);
        }

        protected override void OnModelSuccess(GameObject loadedGO)
        {
            base.OnModelSuccess(loadedGO);
            // if(previousSkin != null)
            //     DestroyImmediate(previousSkin);
            loadedGO.transform.SetParent(gameObject.transform);
        }
        [Button()]
        public void SwapSkinToOrigin()
        {
            Destroy(InstantiatedGO);
            weaponGameObject.SetActive(true);
        }
    }
}