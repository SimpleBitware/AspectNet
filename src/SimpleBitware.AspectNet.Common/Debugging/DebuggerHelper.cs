using System;
using System.Diagnostics;
using System.Threading;

namespace SimpleBitware.AspectNet.Common.Debugging;

public static class DebuggerHelper
{
    public static void WaitForDebuggerToAttach()
    {
        if (!Debugger.IsAttached)
        {
            Debug.WriteLine($"Waiting for debugger. PID: {Process.GetCurrentProcess().Id}");
            while (!Debugger.IsAttached)
                Thread.Sleep(1000);
        }

        Debugger.Break();
    }
}
