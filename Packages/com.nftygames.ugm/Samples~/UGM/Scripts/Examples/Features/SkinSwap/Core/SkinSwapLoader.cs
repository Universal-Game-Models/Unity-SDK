using System.Threading.Tasks;
using NaughtyAttributes;
using UGM.Core;
using UGM.Examples.Features.SkinSwap.Interface;
using UnityEngine;

namespace UGM.Examples.Features.SkinSwap.Core
{
    public class SkinSwapLoader : UGMDownloader, ILoadableSkin, ISwappableSkin
    {
        [SerializeField] [Tooltip("The original gameobject of this item")]
        protected GameObject originalGameObject;

        /// <summary>
        /// Load the item, swap the original game object to the given data.
        /// </summary>
        /// <param name="data"></param>
        public virtual void LoadItem(UGMDataTypes.TokenInfo data)
        {
            LoadItem(data.token_id);
        }

        public virtual void LoadItem(string id)
        {
            Load(id);
            if (originalGameObject == null) return;
            if(originalGameObject.activeSelf == true)
                originalGameObject.SetActive(false);
        }

        protected virtual void OnEnable()
        {
            ExampleUIEvents.OnChangeEquipment.AddListener(LoadItem);
        }

        protected virtual void OnDisable()
        {
            ExampleUIEvents.OnChangeEquipment.RemoveListener(LoadItem);
            SwapToOriginalSkin();
        }

        protected override void OnModelSuccess(GameObject loadedGO)
        {
            base.OnModelSuccess(loadedGO);

            InstantiatedGO.transform.SetParent(gameObject.transform);
        }


        [Button()]
        public virtual void SwapToOriginalSkin()
        {
            if (InstantiatedGO != null)
            {
                DestroyImmediate(InstantiatedGO);
            }
            if (originalGameObject == null) return;
            originalGameObject.SetActive(true);
        }
    }
}