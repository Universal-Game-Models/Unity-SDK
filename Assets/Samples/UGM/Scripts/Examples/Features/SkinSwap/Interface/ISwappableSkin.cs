namespace UGM.Examples.Features.SkinSwap.Interface
{
    public interface ISwappableSkin
    {
        /// <summary>
        /// Destroy the existing skin and activate the original game object.
        /// </summary>
        public void SwapToOriginalSkin();
    }
}