using System.Threading.Tasks;

namespace UGM.Examples.Features.SkinSwap.Interface
{
    public interface ILoadableSkin
    {
        public Task LoadSkin(string id);
    }
}