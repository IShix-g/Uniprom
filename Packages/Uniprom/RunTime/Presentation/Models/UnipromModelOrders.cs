
using System;

namespace Uniprom
{
    public readonly struct UnipromModelOrders : IEquatable<UnipromModelOrders>
    {
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

        public static UnipromModelOrders Default
            => new (UnipromModelOrderType.Random,
                UnipromModelOrderType.InSequence,
                UnipromModelOrderType.Random,
                UnipromModelOrderType.Random);
        
        public bool Equals(UnipromModelOrders other) => Interstitial == other.Interstitial && Wall == other.Wall && Native == other.Native && NativeIcon == other.NativeIcon;
        public override bool Equals(object obj) => obj is UnipromModelOrders other && Equals(other);
        public override int GetHashCode() => HashCode.Combine((int) Interstitial, (int) Wall, (int) Native, (int) NativeIcon);
        public static bool operator ==(UnipromModelOrders lhs, UnipromModelOrders rhs) => lhs.Equals(rhs);
        public static bool operator !=(UnipromModelOrders lhs, UnipromModelOrders rhs) => !(lhs == rhs);
    }
}