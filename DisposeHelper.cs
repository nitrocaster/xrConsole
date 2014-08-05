using System;
using System.Diagnostics;

namespace xr
{
    public static class DisposeHelper
    {
        [Conditional("DEBUG")]
        public static void OnDispose(bool disposing, string className)
        {
            if (disposing)
            {
                return;
            }
            if (!AppDomain.CurrentDomain.IsFinalizingForUnload() && !Environment.HasShutdownStarted)
            {
                Debug.Fail("Non-disposed object finalization: " + className);
            }
        }
    }
}
