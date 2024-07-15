
using System;

namespace Uniprom
{
    [Serializable]
    public readonly struct UnipromViewErrorStatus
    {
        public readonly UnipromViewType ViewType;
        public readonly UnipromViewErrorType ErrorType;
        public readonly Exception Exception;

        public UnipromViewErrorStatus(UnipromViewType viewType, UnipromViewErrorType errorType, Exception exception)
        {
            ViewType = viewType;
            ErrorType = errorType;
            Exception = exception;
        }
        
        public override string ToString()
            => "ViewType: " + ViewType
            + " ErrorType: " + ErrorType
            + " Exception: " + Exception
            + "\n" + base.ToString();
    }
}