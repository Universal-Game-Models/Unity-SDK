using System.Threading.Tasks;
using UGM.Core;
using UGM.Examples.Features.SkinSwap.Interface;
using UnityEngine;

namespace UGM.Examples.Features.SkinSwap.Core
{
    public class SkinSwapLoader : UGMDownloader, ILoadableSkin
    {
        [SerializeField]
        protected UGMDownloader weaponUGM;

        private GameObject weaponGameObject;
        private GameObject previousSkin;
        public async Task LoadSkin(string id)
        {
            if (weaponGameObject == null)
                weaponGameObject = weaponUGM.InstantiatedGO;
            weaponGameObject.SetActive(false);
            await LoadAsync(id);
        }

        protected override void OnModelSuccess(GameObject loadedGO)
        {
            base.OnModelSuccess(loadedGO);
            if(previousSkin != null)
                DestroyImmediate(previousSkin);
            previousSkin = loadedGO;
            loadedGO.transform.SetParent(gameObject.transform);
        }
    }
}