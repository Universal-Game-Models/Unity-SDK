using System.Threading.Tasks;
using UGM.Core;

namespace UGM.Examples.Features.SkinSwap.Interface
{
    public interface ILoadableSkin
    {
        public void LoadSkin(UGMDataTypes.TokenInfo data);
    }
}