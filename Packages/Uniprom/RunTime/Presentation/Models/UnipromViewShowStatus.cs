
using System;

namespace Uniprom
{
    [Serializable]
    public readonly struct UnipromViewShowStatus
    {
        public readonly string AppKey;
        public readonly UnipromViewType ViewType;
        
        public UnipromViewShowStatus(string appKey, UnipromViewType viewType)
        {
            AppKey = appKey;
            ViewType = viewType;
        }

        public override string ToString()
            => "AppKey: " + AppKey
            + " ViewType: " + ViewType
            + "\n" + base.ToString();
    }
}