
namespace Uniprom
{
    public sealed class UnipromModelOrders
    {
        static UnipromModelOrders s_default;

        public static UnipromModelOrders Default
            => s_default ??= new UnipromModelOrders(
                UnipromModelOrderType.Random,
                UnipromModelOrderType.InReverse,
                UnipromModelOrderType.Random,
                UnipromModelOrderType.Random);
        
        public readonly UnipromModelOrderType Interstitial;
        public readonly UnipromModelOrderType Wall;
        public readonly UnipromModelOrderType Native;
        public readonly UnipromModelOrderType NativeIcon;

        public UnipromModelOrders(
            UnipromModelOrderType interstitial,
            UnipromModelOrderType wall,
            UnipromModelOrderType native,
            UnipromModelOrderType nativeIcon)
        {
            Interstitial = interstitial;
            Wall = wall;
            Native = native;
            NativeIcon = nativeIcon;
        }
    }
}