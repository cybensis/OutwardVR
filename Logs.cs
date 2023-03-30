using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OutwardVR
{
    class Logs
    {
        // ReSharper disable Unity.PerformanceAnalysis
        public static void WriteInfo(object data)
        {
#if DEBUG
            Debug.Log(data);
#endif
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public static void WriteWarning(object data)
        {
#if DEBUG
            Debug.LogWarning(data);
#endif
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public static void WriteError(object data)
        {
#if DEBUG
            Debug.LogError(data);
#endif
        }
    }
}
