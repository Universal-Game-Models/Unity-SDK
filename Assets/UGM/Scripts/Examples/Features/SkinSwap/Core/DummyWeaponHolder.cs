using System.Threading.Tasks;
using NaughtyAttributes;
using UGM.Core;
using UGM.Examples.Features.SkinSwap.Interface;
using UnityEngine;

namespace UGM.Examples.Features.SkinSwap.Core
{
    public class DummyWeaponHolder : UGMDownloader, ILoadableSkin
    {
        protected override void Start()
        {
            base.Start();
            LoadSkin("15");
        }

        public async Task LoadSkin(string id)
        {
            await LoadAsync(id);
        }

        protected override void OnModelSuccess(GameObject loadedGO)
        {
            base.OnModelSuccess(loadedGO);
            loadedGO.transform.SetParent(gameObject.transform);
        }
    }
}