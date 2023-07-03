using System.Threading.Tasks;
using UGM.Core;

namespace UGM.Examples.Features.SkinSwap.Interface
{
    public interface ILoadableSkin
    {
        /// <summary>
        /// Load the item, swap the original game object to the given data.
        /// </summary>
        /// <param name="data"></param>
        public void LoadItem(UGMDataTypes.TokenInfo data);
        /// <summary>
        /// Load the item, swap the original game object to the given data.
        /// </summary>
        /// <param name="data"></param>
        public void LoadItem(string id);
    }
}