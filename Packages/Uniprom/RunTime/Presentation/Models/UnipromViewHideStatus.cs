
using System;

namespace Uniprom
{
    [Serializable]
    public readonly struct UnipromViewHideStatus
    {
        public readonly string AppKey;
        public readonly UnipromViewType ViewType;
        public readonly bool IsClicked;

        public UnipromViewHideStatus(string appKey, UnipromViewType viewType, bool isClicked)
        {
            AppKey = appKey;
            ViewType = viewType;
            IsClicked = isClicked;
        }
        
        public override string ToString()
            => "AppKey: " + AppKey
            + " ViewType: " + ViewType
            + " IsClicked: " + IsClicked
            + "\n" + base.ToString();
    }
}