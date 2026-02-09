using System.Diagnostics;
using System.Threading;
using Microsoft.CodeAnalysis;
using SimpleBitware.AspectNet.Common.Extensions;

namespace SimpleBitware.AspectNet.Common.Debugging;

public static class DebuggerHelper
{
    public static void WaitForDebuggerToAttach(SourceProductionContext context)
    {
        if (!Debugger.IsAttached)
        {
            context.WriteLine($"Waiting for debugger. PID: {Process.GetCurrentProcess().Id}");
            while (!Debugger.IsAttached)
                Thread.Sleep(1000);
        }

        Debugger.Break();
    }
}
