#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;

namespace Uniprom.Editor
{
    public sealed class UnipromDebug
    {
        public static bool IsBatchMode;
        
        public static void Log(string msg)
        {
            if (IsBatchMode) Console.WriteLine(msg);
            else Debug.Log(msg);
        }

        public static void LogWarning(string msg)
        {
            if (IsBatchMode) Console.WriteLine($"::warning:: {msg}");
            else Debug.LogWarning(msg);
        }

        public static void LogError(string msg)
        {
            if (IsBatchMode)
            {
                Console.WriteLine($"::error:: {msg}");
                EditorApplication.Exit(100);
            }
            else Debug.LogError(msg);
        }
    }
}
#endif