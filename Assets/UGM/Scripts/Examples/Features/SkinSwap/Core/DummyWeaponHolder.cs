using System;
using System.Threading.Tasks;
using NaughtyAttributes;
using UGM.Core;
using UGM.Examples.Features.SkinSwap.Interface;
using UnityEngine;

namespace UGM.Examples.Features.SkinSwap.Core
{
    public class DummyWeaponHolder : UGMDownloader, ILoadableSkin, ITokenable
    {

        public UGMDataTypes.TokenInfo TokenData { get; set; }
        protected override void Start()
        {
            base.Start();
            // LoadSkin("15");
            
        }

        private void OnEnable()
        {
            ExampleUIEvents.OnChangeEquipment.AddListener(LoadSkin);
        }

        private void OnDisable()
        {
            ExampleUIEvents.OnChangeEquipment.RemoveListener(LoadSkin);
        }

        public void LoadSkin(UGMDataTypes.TokenInfo data)
        {
            LoadAsync(data.token_id);
            TokenData = data;
        }

        protected override void OnModelSuccess(GameObject loadedGO)
        {
            base.OnModelSuccess(loadedGO);
            loadedGO.transform.SetParent(gameObject.transform);
        }
        
    }
}