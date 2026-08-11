using UnityEngine;

namespace Pie.Core
{
    public class Bootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void Initialization()
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = (int)Screen.currentResolution.refreshRateRatio.value;
        }
    }
}