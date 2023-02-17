using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Log
{
    public static class MyLogger
    {
        [Conditional("ENABLE_DEBUG_LOG")]
        public static void Log(string content)
        {
            Debug.Log(content);
        }
    }
}
