using System.Diagnostics;

namespace Cosm.Net.Generators;
public static class DebuggerUtil
{
    private static bool LaunchedBefore = false;

    public static void Attach()
    {
#if DEBUG
        if(!Debugger.IsAttached && !LaunchedBefore)
        {
            LaunchedBefore = true;
            Debugger.Launch();
        }
#endif
    }
}
