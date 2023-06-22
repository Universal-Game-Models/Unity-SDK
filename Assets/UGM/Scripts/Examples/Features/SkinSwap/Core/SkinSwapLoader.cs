using System.Threading.Tasks;
using NaughtyAttributes;
using UGM.Core;
using UGM.Examples.Features.SkinSwap.Interface;
using UnityEngine;

namespace UGM.Examples.Features.SkinSwap.Core
{
    public class SkinSwapLoader : UGMDownloader, ILoadableSkin, ITokenable, ISwappableSkin
    {
        private GameObject weaponGameObject;
        private string defaultId;
        public UGMDataTypes.TokenInfo OriginalItemTokenData { get; set; }
        
        public void LoadSkin(UGMDataTypes.TokenInfo data)
        {
            Load(data.token_id);
        }

        protected virtual void LoadItem(UGMDataTypes.TokenInfo data)
        {
            OriginalItemTokenData = data;
            Load(data.token_id);
        }
        private void OnEnable()
        {
            ExampleUIEvents.OnChangeEquipment.AddListener(LoadItem);
        }

        private void OnDisable()
        {
            ExampleUIEvents.OnChangeEquipment.RemoveListener(LoadItem);
        }

        protected override void OnModelSuccess(GameObject loadedGO)
        {
            base.OnModelSuccess(loadedGO);

            InstantiatedGO.transform.SetParent(gameObject.transform);
        }

        [Button()]
        public void SwapToOriginalSkin()
        {
            LoadSkin(OriginalItemTokenData);
        }
    }
}