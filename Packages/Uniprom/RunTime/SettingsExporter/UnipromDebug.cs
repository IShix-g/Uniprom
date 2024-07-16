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
            if (IsBatchMode) Console.WriteLine("[Uniprom] " + msg);
            else Debug.Log("[Uniprom] " + msg);
        }

        public static void LogWarning(string msg)
        {
            if (IsBatchMode) Console.WriteLine($"::warning:: [Uniprom] {msg}");
            else Debug.LogWarning(msg);
        }

        public static void LogError(string msg)
        {
            if (IsBatchMode)
            {
                Console.WriteLine($"::error:: [Uniprom] {msg}");
                EditorApplication.Exit(222);
            }
            else Debug.LogError(msg);
        }
    }
}
#endif