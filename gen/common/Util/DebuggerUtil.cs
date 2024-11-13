using System.Diagnostics;

namespace Cosm.Net.Generators.Common.Util;
public static class DebuggerUtils
{
#if DEBUG
    private static bool LaunchedBefore = false;

    public static void Attach()
    {

        if(!Debugger.IsAttached && !LaunchedBefore)
        {
            LaunchedBefore = true;
            Debugger.Launch();
        }
}
#endif
}
