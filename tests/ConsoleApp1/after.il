// Type: <Module>
// Type: Program

.method System.Void Program::<Main>$(System.String[])
0000: ldarg.0 
0001: call Microsoft.Extensions.Hosting.HostApplicationBuilder Microsoft.Extensions.Hosting.Host::CreateApplicationBuilder(System.String[])
0006: stloc.0 
0007: ldloc.0 
0008: callvirt Microsoft.Extensions.DependencyInjection.IServiceCollection Microsoft.Extensions.Hosting.HostApplicationBuilder::get_Services()
000D: call Microsoft.Extensions.DependencyInjection.IServiceCollection Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions::AddSingleton<ConsoleApp1.TestClassX>(Microsoft.Extensions.DependencyInjection.IServiceCollection)
0012: pop 
0013: ldloc.0 
0014: callvirt Microsoft.Extensions.Hosting.IHost Microsoft.Extensions.Hosting.HostApplicationBuilder::Build()
0019: stloc.1 
001A: ldloc.1 
001B: callvirt System.IServiceProvider Microsoft.Extensions.Hosting.IHost::get_Services()
0020: call System.IServiceProvider SimpleBitware.AspectNet.Extensions.DependencyInjection.ServiceProviderExtensions::UseAspectNet(System.IServiceProvider)
0025: pop 
0026: ldstr FROM CONSTRUCTOR
002B: call System.Void System.Console::WriteLine(System.String)
0030: nop 
0031: ldc.i4.2 
0032: newobj System.Void ConsoleApp1.TestClassX::.ctor(System.Int32)
0037: stloc.2 
0038: ldloc.2 
0039: ldstr Constructor
003E: ldc.i4.1 
003F: callvirt System.Void ConsoleApp1.TestClassX::LogMe(System.String,System.Int32)
0044: nop 
0045: nop 
0046: ldstr static async
004B: call System.Threading.Tasks.Task ConsoleApp1.TestClassX::LogMe(System.String)
0050: callvirt System.Void System.Threading.Tasks.Task::Wait()
0055: nop 
0056: nop 
0057: leave.s IL_005f: ldloca.s V_5
0059: stloc.s V_4
005B: nop 
005C: nop 
005D: leave.s IL_005f: ldloca.s V_5
005F: ldloca.s V_5
0061: ldc.i4.3 
0062: ldc.i4.1 
0063: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::.ctor(System.Int32,System.Int32)
0068: ldloca.s V_5
006A: ldstr No=
006F: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::AppendLiteral(System.String)
0074: nop 
0075: ldloca.s V_5
0077: ldloc.2 
0078: callvirt System.Int32 ConsoleApp1.TestClassX::get_No()
007D: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::AppendFormatted<System.Int32>(!!0)
0082: nop 
0083: ldloca.s V_5
0085: call System.String System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::ToStringAndClear()
008A: call System.Void System.Console::WriteLine(System.String)
008F: nop 
0090: ldstr ------------------
0095: call System.Void System.Console::WriteLine(System.String)
009A: nop 
009B: ldstr FROM DI
00A0: call System.Void System.Console::WriteLine(System.String)
00A5: nop 
00A6: ldloc.1 
00A7: callvirt System.IServiceProvider Microsoft.Extensions.Hosting.IHost::get_Services()
00AC: call !!0 Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions::GetRequiredService<ConsoleApp1.TestClassX>(System.IServiceProvider)
00B1: stloc.3 
00B2: ldloc.3 
00B3: ldstr DI
00B8: ldc.i4.2 
00B9: callvirt System.Void ConsoleApp1.TestClassX::LogMe(System.String,System.Int32)
00BE: nop 
00BF: ldloca.s V_6
00C1: ldc.i4.3 
00C2: ldc.i4.1 
00C3: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::.ctor(System.Int32,System.Int32)
00C8: ldloca.s V_6
00CA: ldstr No=
00CF: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::AppendLiteral(System.String)
00D4: nop 
00D5: ldloca.s V_6
00D7: ldloc.3 
00D8: callvirt System.Int32 ConsoleApp1.TestClassX::get_No()
00DD: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::AppendFormatted<System.Int32>(!!0)
00E2: nop 
00E3: ldloca.s V_6
00E5: call System.String System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::ToStringAndClear()
00EA: call System.Void System.Console::WriteLine(System.String)
00EF: nop 
00F0: ret 


.method System.Void Program::.ctor()
0000: ldarg.0 
0001: call System.Void System.Object::.ctor()
0006: nop 
0007: ret 

// Type: ConsoleApp1.Log2Attribute

.method System.Void ConsoleApp1.Log2Attribute::OnEntry(SimpleBitware.AspectNet.Abstractions.AspectNetEntryContext)
0000: nop 
0001: ldloca.s V_0
0003: ldc.i4.s 26
0005: ldc.i4.3 
0006: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::.ctor(System.Int32,System.Int32)
000B: ldloca.s V_0
000D: ldstr OnEntry2: 
0012: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::AppendLiteral(System.String)
0017: nop 
0018: ldloca.s V_0
001A: ldarg.1 
001B: callvirt System.String SimpleBitware.AspectNet.Abstractions.AbstractAspectNetContext::get_ClassName()
0020: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::AppendFormatted(System.String)
0025: nop 
0026: ldloca.s V_0
0028: ldstr , 
002D: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::AppendLiteral(System.String)
0032: nop 
0033: ldloca.s V_0
0035: ldarg.1 
0036: callvirt System.String SimpleBitware.AspectNet.Abstractions.AbstractAspectNetContext::get_MemberName()
003B: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::AppendFormatted(System.String)
0040: nop 
0041: ldloca.s V_0
0043: ldstr , Parameters: 
0048: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::AppendLiteral(System.String)
004D: nop 
004E: ldloca.s V_0
0050: ldstr , 
0055: ldarg.1 
0056: callvirt System.Collections.Generic.Dictionary`2<System.String,System.Object> SimpleBitware.AspectNet.Abstractions.AbstractAspectNetContext::get_Parameters()
005B: ldsfld System.Func`2<System.Collections.Generic.KeyValuePair`2<System.String,System.Object>,System.String> ConsoleApp1.Log2Attribute/<>c::<>9__0_0
0060: dup 
0061: brtrue.s IL_007a: call System.Collections.Generic.IEnumerable`1<!!1> System.Linq.Enumerable::Select<System.Collections.Generic.KeyValuePair`2<System.String,System.Object>,System.String>(System.Collections.Generic.IEnumerable`1<!!0>,System.Func`2<!!0,!!1>)
0063: pop 
0064: ldsfld ConsoleApp1.Log2Attribute/<>c ConsoleApp1.Log2Attribute/<>c::<>9
0069: ldftn System.String ConsoleApp1.Log2Attribute/<>c::<OnEntry>b__0_0(System.Collections.Generic.KeyValuePair`2<System.String,System.Object>)
006F: newobj System.Void System.Func`2<System.Collections.Generic.KeyValuePair`2<System.String,System.Object>,System.String>::.ctor(System.Object,System.IntPtr)
0074: dup 
0075: stsfld System.Func`2<System.Collections.Generic.KeyValuePair`2<System.String,System.Object>,System.String> ConsoleApp1.Log2Attribute/<>c::<>9__0_0
007A: call System.Collections.Generic.IEnumerable`1<!!1> System.Linq.Enumerable::Select<System.Collections.Generic.KeyValuePair`2<System.String,System.Object>,System.String>(System.Collections.Generic.IEnumerable`1<!!0>,System.Func`2<!!0,!!1>)
007F: call System.String System.String::Join(System.String,System.Collections.Generic.IEnumerable`1<System.String>)
0084: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::AppendFormatted(System.String)
0089: nop 
008A: ldloca.s V_0
008C: call System.String System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::ToStringAndClear()
0091: call System.Void System.Console::WriteLine(System.String)
0096: nop 
0097: ldarg.0 
0098: ldarg.1 
0099: call System.Void SimpleBitware.AspectNet.Abstractions.AbstractAspectNetAttribute::OnEntry(SimpleBitware.AspectNet.Abstractions.AspectNetEntryContext)
009E: nop 
009F: ret 


.method System.Void ConsoleApp1.Log2Attribute::OnExit(SimpleBitware.AspectNet.Abstractions.AspectNetExitContext)
0000: nop 
0001: ldloca.s V_0
0003: ldc.i4.s 40
0005: ldc.i4.4 
0006: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::.ctor(System.Int32,System.Int32)
000B: ldloca.s V_0
000D: ldstr OnExit2: 
0012: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::AppendLiteral(System.String)
0017: nop 
0018: ldloca.s V_0
001A: ldarg.1 
001B: callvirt System.String SimpleBitware.AspectNet.Abstractions.AbstractAspectNetContext::get_ClassName()
0020: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::AppendFormatted(System.String)
0025: nop 
0026: ldloca.s V_0
0028: ldstr , 
002D: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::AppendLiteral(System.String)
0032: nop 
0033: ldloca.s V_0
0035: ldarg.1 
0036: callvirt System.String SimpleBitware.AspectNet.Abstractions.AbstractAspectNetContext::get_MemberName()
003B: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::AppendFormatted(System.String)
0040: nop 
0041: ldloca.s V_0
0043: ldstr , Parameters: 
0048: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::AppendLiteral(System.String)
004D: nop 
004E: ldloca.s V_0
0050: ldstr , 
0055: ldarg.1 
0056: callvirt System.Collections.Generic.Dictionary`2<System.String,System.Object> SimpleBitware.AspectNet.Abstractions.AbstractAspectNetContext::get_Parameters()
005B: ldsfld System.Func`2<System.Collections.Generic.KeyValuePair`2<System.String,System.Object>,System.String> ConsoleApp1.Log2Attribute/<>c::<>9__1_0
0060: dup 
0061: brtrue.s IL_007a: call System.Collections.Generic.IEnumerable`1<!!1> System.Linq.Enumerable::Select<System.Collections.Generic.KeyValuePair`2<System.String,System.Object>,System.String>(System.Collections.Generic.IEnumerable`1<!!0>,System.Func`2<!!0,!!1>)
0063: pop 
0064: ldsfld ConsoleApp1.Log2Attribute/<>c ConsoleApp1.Log2Attribute/<>c::<>9
0069: ldftn System.String ConsoleApp1.Log2Attribute/<>c::<OnExit>b__1_0(System.Collections.Generic.KeyValuePair`2<System.String,System.Object>)
006F: newobj System.Void System.Func`2<System.Collections.Generic.KeyValuePair`2<System.String,System.Object>,System.String>::.ctor(System.Object,System.IntPtr)
0074: dup 
0075: stsfld System.Func`2<System.Collections.Generic.KeyValuePair`2<System.String,System.Object>,System.String> ConsoleApp1.Log2Attribute/<>c::<>9__1_0
007A: call System.Collections.Generic.IEnumerable`1<!!1> System.Linq.Enumerable::Select<System.Collections.Generic.KeyValuePair`2<System.String,System.Object>,System.String>(System.Collections.Generic.IEnumerable`1<!!0>,System.Func`2<!!0,!!1>)
007F: call System.String System.String::Join(System.String,System.Collections.Generic.IEnumerable`1<System.String>)
0084: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::AppendFormatted(System.String)
0089: nop 
008A: ldloca.s V_0
008C: ldstr , ReturnValue: 
0091: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::AppendLiteral(System.String)
0096: nop 
0097: ldloca.s V_0
0099: ldarg.1 
009A: callvirt System.Object SimpleBitware.AspectNet.Abstractions.AspectNetExitContext::get_ReturnValue()
009F: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::AppendFormatted<System.Object>(!!0)
00A4: nop 
00A5: ldloca.s V_0
00A7: call System.String System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::ToStringAndClear()
00AC: call System.Void System.Console::WriteLine(System.String)
00B1: nop 
00B2: ldarg.0 
00B3: ldarg.1 
00B4: call System.Void SimpleBitware.AspectNet.Abstractions.AbstractAspectNetAttribute::OnExit(SimpleBitware.AspectNet.Abstractions.AspectNetExitContext)
00B9: nop 
00BA: ret 


.method System.Void ConsoleApp1.Log2Attribute::OnException(SimpleBitware.AspectNet.Abstractions.AspectNetExceptionContext)
0000: nop 
0001: ldloca.s V_0
0003: ldc.i4.s 43
0005: ldc.i4.4 
0006: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::.ctor(System.Int32,System.Int32)
000B: ldloca.s V_0
000D: ldstr OnException2: 
0012: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::AppendLiteral(System.String)
0017: nop 
0018: ldloca.s V_0
001A: ldarg.1 
001B: callvirt System.String SimpleBitware.AspectNet.Abstractions.AbstractAspectNetContext::get_ClassName()
0020: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::AppendFormatted(System.String)
0025: nop 
0026: ldloca.s V_0
0028: ldstr , 
002D: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::AppendLiteral(System.String)
0032: nop 
0033: ldloca.s V_0
0035: ldarg.1 
0036: callvirt System.String SimpleBitware.AspectNet.Abstractions.AbstractAspectNetContext::get_MemberName()
003B: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::AppendFormatted(System.String)
0040: nop 
0041: ldloca.s V_0
0043: ldstr , Parameters: 
0048: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::AppendLiteral(System.String)
004D: nop 
004E: ldloca.s V_0
0050: ldstr , 
0055: ldarg.1 
0056: callvirt System.Collections.Generic.Dictionary`2<System.String,System.Object> SimpleBitware.AspectNet.Abstractions.AbstractAspectNetContext::get_Parameters()
005B: ldsfld System.Func`2<System.Collections.Generic.KeyValuePair`2<System.String,System.Object>,System.String> ConsoleApp1.Log2Attribute/<>c::<>9__2_0
0060: dup 
0061: brtrue.s IL_007a: call System.Collections.Generic.IEnumerable`1<!!1> System.Linq.Enumerable::Select<System.Collections.Generic.KeyValuePair`2<System.String,System.Object>,System.String>(System.Collections.Generic.IEnumerable`1<!!0>,System.Func`2<!!0,!!1>)
0063: pop 
0064: ldsfld ConsoleApp1.Log2Attribute/<>c ConsoleApp1.Log2Attribute/<>c::<>9
0069: ldftn System.String ConsoleApp1.Log2Attribute/<>c::<OnException>b__2_0(System.Collections.Generic.KeyValuePair`2<System.String,System.Object>)
006F: newobj System.Void System.Func`2<System.Collections.Generic.KeyValuePair`2<System.String,System.Object>,System.String>::.ctor(System.Object,System.IntPtr)
0074: dup 
0075: stsfld System.Func`2<System.Collections.Generic.KeyValuePair`2<System.String,System.Object>,System.String> ConsoleApp1.Log2Attribute/<>c::<>9__2_0
007A: call System.Collections.Generic.IEnumerable`1<!!1> System.Linq.Enumerable::Select<System.Collections.Generic.KeyValuePair`2<System.String,System.Object>,System.String>(System.Collections.Generic.IEnumerable`1<!!0>,System.Func`2<!!0,!!1>)
007F: call System.String System.String::Join(System.String,System.Collections.Generic.IEnumerable`1<System.String>)
0084: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::AppendFormatted(System.String)
0089: nop 
008A: ldloca.s V_0
008C: ldstr , Exception: 
0091: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::AppendLiteral(System.String)
0096: nop 
0097: ldloca.s V_0
0099: ldarg.1 
009A: callvirt System.Exception SimpleBitware.AspectNet.Abstractions.AspectNetExceptionContext::get_Exception()
009F: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::AppendFormatted<System.Exception>(!!0)
00A4: nop 
00A5: ldloca.s V_0
00A7: call System.String System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::ToStringAndClear()
00AC: call System.Void System.Console::WriteLine(System.String)
00B1: nop 
00B2: ldarg.0 
00B3: ldarg.1 
00B4: call System.Void SimpleBitware.AspectNet.Abstractions.AbstractAspectNetAttribute::OnException(SimpleBitware.AspectNet.Abstractions.AspectNetExceptionContext)
00B9: nop 
00BA: ret 


.method System.Void ConsoleApp1.Log2Attribute::.ctor()
0000: ldarg.0 
0001: call System.Void SimpleBitware.AspectNet.Abstractions.AbstractAspectNetAttribute::.ctor()
0006: nop 
0007: ret 

// Type: ConsoleApp1.LogAttribute

.method System.Void ConsoleApp1.LogAttribute::OnEntry(SimpleBitware.AspectNet.Abstractions.AspectNetEntryContext)
0000: nop 
0001: ldloca.s V_0
0003: ldc.i4.s 25
0005: ldc.i4.3 
0006: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::.ctor(System.Int32,System.Int32)
000B: ldloca.s V_0
000D: ldstr OnEntry: 
0012: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::AppendLiteral(System.String)
0017: nop 
0018: ldloca.s V_0
001A: ldarg.1 
001B: callvirt System.String SimpleBitware.AspectNet.Abstractions.AbstractAspectNetContext::get_ClassName()
0020: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::AppendFormatted(System.String)
0025: nop 
0026: ldloca.s V_0
0028: ldstr , 
002D: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::AppendLiteral(System.String)
0032: nop 
0033: ldloca.s V_0
0035: ldarg.1 
0036: callvirt System.String SimpleBitware.AspectNet.Abstractions.AbstractAspectNetContext::get_MemberName()
003B: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::AppendFormatted(System.String)
0040: nop 
0041: ldloca.s V_0
0043: ldstr , Parameters: 
0048: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::AppendLiteral(System.String)
004D: nop 
004E: ldloca.s V_0
0050: ldstr , 
0055: ldarg.1 
0056: callvirt System.Collections.Generic.Dictionary`2<System.String,System.Object> SimpleBitware.AspectNet.Abstractions.AbstractAspectNetContext::get_Parameters()
005B: ldsfld System.Func`2<System.Collections.Generic.KeyValuePair`2<System.String,System.Object>,System.String> ConsoleApp1.LogAttribute/<>c::<>9__0_0
0060: dup 
0061: brtrue.s IL_007a: call System.Collections.Generic.IEnumerable`1<!!1> System.Linq.Enumerable::Select<System.Collections.Generic.KeyValuePair`2<System.String,System.Object>,System.String>(System.Collections.Generic.IEnumerable`1<!!0>,System.Func`2<!!0,!!1>)
0063: pop 
0064: ldsfld ConsoleApp1.LogAttribute/<>c ConsoleApp1.LogAttribute/<>c::<>9
0069: ldftn System.String ConsoleApp1.LogAttribute/<>c::<OnEntry>b__0_0(System.Collections.Generic.KeyValuePair`2<System.String,System.Object>)
006F: newobj System.Void System.Func`2<System.Collections.Generic.KeyValuePair`2<System.String,System.Object>,System.String>::.ctor(System.Object,System.IntPtr)
0074: dup 
0075: stsfld System.Func`2<System.Collections.Generic.KeyValuePair`2<System.String,System.Object>,System.String> ConsoleApp1.LogAttribute/<>c::<>9__0_0
007A: call System.Collections.Generic.IEnumerable`1<!!1> System.Linq.Enumerable::Select<System.Collections.Generic.KeyValuePair`2<System.String,System.Object>,System.String>(System.Collections.Generic.IEnumerable`1<!!0>,System.Func`2<!!0,!!1>)
007F: call System.String System.String::Join(System.String,System.Collections.Generic.IEnumerable`1<System.String>)
0084: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::AppendFormatted(System.String)
0089: nop 
008A: ldloca.s V_0
008C: call System.String System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::ToStringAndClear()
0091: call System.Void System.Console::WriteLine(System.String)
0096: nop 
0097: ldarg.0 
0098: ldarg.1 
0099: call System.Void SimpleBitware.AspectNet.Abstractions.AbstractAspectNetAttribute::OnEntry(SimpleBitware.AspectNet.Abstractions.AspectNetEntryContext)
009E: nop 
009F: ret 


.method System.Void ConsoleApp1.LogAttribute::OnExit(SimpleBitware.AspectNet.Abstractions.AspectNetExitContext)
0000: nop 
0001: ldloca.s V_0
0003: ldc.i4.s 39
0005: ldc.i4.4 
0006: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::.ctor(System.Int32,System.Int32)
000B: ldloca.s V_0
000D: ldstr OnExit: 
0012: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::AppendLiteral(System.String)
0017: nop 
0018: ldloca.s V_0
001A: ldarg.1 
001B: callvirt System.String SimpleBitware.AspectNet.Abstractions.AbstractAspectNetContext::get_ClassName()
0020: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::AppendFormatted(System.String)
0025: nop 
0026: ldloca.s V_0
0028: ldstr , 
002D: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::AppendLiteral(System.String)
0032: nop 
0033: ldloca.s V_0
0035: ldarg.1 
0036: callvirt System.String SimpleBitware.AspectNet.Abstractions.AbstractAspectNetContext::get_MemberName()
003B: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::AppendFormatted(System.String)
0040: nop 
0041: ldloca.s V_0
0043: ldstr , Parameters: 
0048: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::AppendLiteral(System.String)
004D: nop 
004E: ldloca.s V_0
0050: ldstr , 
0055: ldarg.1 
0056: callvirt System.Collections.Generic.Dictionary`2<System.String,System.Object> SimpleBitware.AspectNet.Abstractions.AbstractAspectNetContext::get_Parameters()
005B: ldsfld System.Func`2<System.Collections.Generic.KeyValuePair`2<System.String,System.Object>,System.String> ConsoleApp1.LogAttribute/<>c::<>9__1_0
0060: dup 
0061: brtrue.s IL_007a: call System.Collections.Generic.IEnumerable`1<!!1> System.Linq.Enumerable::Select<System.Collections.Generic.KeyValuePair`2<System.String,System.Object>,System.String>(System.Collections.Generic.IEnumerable`1<!!0>,System.Func`2<!!0,!!1>)
0063: pop 
0064: ldsfld ConsoleApp1.LogAttribute/<>c ConsoleApp1.LogAttribute/<>c::<>9
0069: ldftn System.String ConsoleApp1.LogAttribute/<>c::<OnExit>b__1_0(System.Collections.Generic.KeyValuePair`2<System.String,System.Object>)
006F: newobj System.Void System.Func`2<System.Collections.Generic.KeyValuePair`2<System.String,System.Object>,System.String>::.ctor(System.Object,System.IntPtr)
0074: dup 
0075: stsfld System.Func`2<System.Collections.Generic.KeyValuePair`2<System.String,System.Object>,System.String> ConsoleApp1.LogAttribute/<>c::<>9__1_0
007A: call System.Collections.Generic.IEnumerable`1<!!1> System.Linq.Enumerable::Select<System.Collections.Generic.KeyValuePair`2<System.String,System.Object>,System.String>(System.Collections.Generic.IEnumerable`1<!!0>,System.Func`2<!!0,!!1>)
007F: call System.String System.String::Join(System.String,System.Collections.Generic.IEnumerable`1<System.String>)
0084: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::AppendFormatted(System.String)
0089: nop 
008A: ldloca.s V_0
008C: ldstr , ReturnValue: 
0091: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::AppendLiteral(System.String)
0096: nop 
0097: ldloca.s V_0
0099: ldarg.1 
009A: callvirt System.Object SimpleBitware.AspectNet.Abstractions.AspectNetExitContext::get_ReturnValue()
009F: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::AppendFormatted<System.Object>(!!0)
00A4: nop 
00A5: ldloca.s V_0
00A7: call System.String System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::ToStringAndClear()
00AC: call System.Void System.Console::WriteLine(System.String)
00B1: nop 
00B2: ldarg.0 
00B3: ldarg.1 
00B4: call System.Void SimpleBitware.AspectNet.Abstractions.AbstractAspectNetAttribute::OnExit(SimpleBitware.AspectNet.Abstractions.AspectNetExitContext)
00B9: nop 
00BA: ret 


.method System.Void ConsoleApp1.LogAttribute::OnException(SimpleBitware.AspectNet.Abstractions.AspectNetExceptionContext)
0000: nop 
0001: ldloca.s V_0
0003: ldc.i4.s 42
0005: ldc.i4.4 
0006: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::.ctor(System.Int32,System.Int32)
000B: ldloca.s V_0
000D: ldstr OnException: 
0012: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::AppendLiteral(System.String)
0017: nop 
0018: ldloca.s V_0
001A: ldarg.1 
001B: callvirt System.String SimpleBitware.AspectNet.Abstractions.AbstractAspectNetContext::get_ClassName()
0020: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::AppendFormatted(System.String)
0025: nop 
0026: ldloca.s V_0
0028: ldstr , 
002D: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::AppendLiteral(System.String)
0032: nop 
0033: ldloca.s V_0
0035: ldarg.1 
0036: callvirt System.String SimpleBitware.AspectNet.Abstractions.AbstractAspectNetContext::get_MemberName()
003B: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::AppendFormatted(System.String)
0040: nop 
0041: ldloca.s V_0
0043: ldstr , Parameters: 
0048: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::AppendLiteral(System.String)
004D: nop 
004E: ldloca.s V_0
0050: ldstr , 
0055: ldarg.1 
0056: callvirt System.Collections.Generic.Dictionary`2<System.String,System.Object> SimpleBitware.AspectNet.Abstractions.AbstractAspectNetContext::get_Parameters()
005B: ldsfld System.Func`2<System.Collections.Generic.KeyValuePair`2<System.String,System.Object>,System.String> ConsoleApp1.LogAttribute/<>c::<>9__2_0
0060: dup 
0061: brtrue.s IL_007a: call System.Collections.Generic.IEnumerable`1<!!1> System.Linq.Enumerable::Select<System.Collections.Generic.KeyValuePair`2<System.String,System.Object>,System.String>(System.Collections.Generic.IEnumerable`1<!!0>,System.Func`2<!!0,!!1>)
0063: pop 
0064: ldsfld ConsoleApp1.LogAttribute/<>c ConsoleApp1.LogAttribute/<>c::<>9
0069: ldftn System.String ConsoleApp1.LogAttribute/<>c::<OnException>b__2_0(System.Collections.Generic.KeyValuePair`2<System.String,System.Object>)
006F: newobj System.Void System.Func`2<System.Collections.Generic.KeyValuePair`2<System.String,System.Object>,System.String>::.ctor(System.Object,System.IntPtr)
0074: dup 
0075: stsfld System.Func`2<System.Collections.Generic.KeyValuePair`2<System.String,System.Object>,System.String> ConsoleApp1.LogAttribute/<>c::<>9__2_0
007A: call System.Collections.Generic.IEnumerable`1<!!1> System.Linq.Enumerable::Select<System.Collections.Generic.KeyValuePair`2<System.String,System.Object>,System.String>(System.Collections.Generic.IEnumerable`1<!!0>,System.Func`2<!!0,!!1>)
007F: call System.String System.String::Join(System.String,System.Collections.Generic.IEnumerable`1<System.String>)
0084: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::AppendFormatted(System.String)
0089: nop 
008A: ldloca.s V_0
008C: ldstr , Exception: 
0091: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::AppendLiteral(System.String)
0096: nop 
0097: ldloca.s V_0
0099: ldarg.1 
009A: callvirt System.Exception SimpleBitware.AspectNet.Abstractions.AspectNetExceptionContext::get_Exception()
009F: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::AppendFormatted<System.Exception>(!!0)
00A4: nop 
00A5: ldloca.s V_0
00A7: call System.String System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::ToStringAndClear()
00AC: call System.Void System.Console::WriteLine(System.String)
00B1: nop 
00B2: ldarg.0 
00B3: ldarg.1 
00B4: call System.Void SimpleBitware.AspectNet.Abstractions.AbstractAspectNetAttribute::OnException(SimpleBitware.AspectNet.Abstractions.AspectNetExceptionContext)
00B9: nop 
00BA: ret 


.method System.Void ConsoleApp1.LogAttribute::.ctor()
0000: ldarg.0 
0001: call System.Void SimpleBitware.AspectNet.Abstractions.AbstractAspectNetAttribute::.ctor()
0006: nop 
0007: ret 

// Type: ConsoleApp1.TestClassX

.method System.Void ConsoleApp1.TestClassX::.ctor(System.Int32)
0000: ldarg.0 
0001: call System.Void System.Object::.ctor()
0006: newobj System.Void ConsoleApp1.LogAttribute::.ctor()
000B: stloc.s V_6
000D: newobj System.Void SimpleBitware.AspectNet.Abstractions.AspectNetExitContext::.ctor()
0012: stloc.s V_7
0014: ldloc.s V_7
0016: ldstr ConsoleApp1.TestClassX
001B: callvirt System.Void modreq(System.Runtime.CompilerServices.IsExternalInit) SimpleBitware.AspectNet.Abstractions.AbstractAspectNetContext::set_ClassName(System.String)
0020: ldloc.s V_7
0022: ldstr .ctor
0027: callvirt System.Void modreq(System.Runtime.CompilerServices.IsExternalInit) SimpleBitware.AspectNet.Abstractions.AbstractAspectNetContext::set_MemberName(System.String)
002C: ldloc.s V_7
002E: newobj System.Void System.Collections.Generic.Dictionary`2<System.String,System.Object>::.ctor()
0033: callvirt System.Void modreq(System.Runtime.CompilerServices.IsExternalInit) SimpleBitware.AspectNet.Abstractions.AbstractAspectNetContext::set_Parameters(System.Collections.Generic.Dictionary`2<System.String,System.Object>)
0038: ldloc.s V_7
003A: callvirt System.Collections.Generic.Dictionary`2<System.String,System.Object> SimpleBitware.AspectNet.Abstractions.AbstractAspectNetContext::get_Parameters()
003F: ldstr no
0044: ldarg.1 
0045: box System.Int32
004A: callvirt System.Void System.Collections.Generic.Dictionary`2<System.String,System.Object>::Add(TKey,TValue)
004F: nop 
0050: ldloc.s V_6
0052: ldloc.s V_7
0054: callvirt System.Void SimpleBitware.AspectNet.Abstractions.AbstractAspectNetAttribute::OnEntry(SimpleBitware.AspectNet.Abstractions.AspectNetEntryContext)
0059: newobj System.Void ConsoleApp1.Log2Attribute::.ctor()
005E: stloc.1 
005F: newobj System.Void SimpleBitware.AspectNet.Abstractions.AspectNetExitContext::.ctor()
0064: stloc.2 
0065: ldloc.2 
0066: ldstr ConsoleApp1.TestClassX
006B: callvirt System.Void modreq(System.Runtime.CompilerServices.IsExternalInit) SimpleBitware.AspectNet.Abstractions.AbstractAspectNetContext::set_ClassName(System.String)
0070: ldloc.2 
0071: ldstr .ctor
0076: callvirt System.Void modreq(System.Runtime.CompilerServices.IsExternalInit) SimpleBitware.AspectNet.Abstractions.AbstractAspectNetContext::set_MemberName(System.String)
007B: ldloc.2 
007C: newobj System.Void System.Collections.Generic.Dictionary`2<System.String,System.Object>::.ctor()
0081: callvirt System.Void modreq(System.Runtime.CompilerServices.IsExternalInit) SimpleBitware.AspectNet.Abstractions.AbstractAspectNetContext::set_Parameters(System.Collections.Generic.Dictionary`2<System.String,System.Object>)
0086: ldloc.2 
0087: callvirt System.Collections.Generic.Dictionary`2<System.String,System.Object> SimpleBitware.AspectNet.Abstractions.AbstractAspectNetContext::get_Parameters()
008C: ldstr no
0091: ldarg.1 
0092: box System.Int32
0097: callvirt System.Void System.Collections.Generic.Dictionary`2<System.String,System.Object>::Add(TKey,TValue)
009C: nop 
009D: ldloc.1 
009E: ldloc.2 
009F: callvirt System.Void SimpleBitware.AspectNet.Abstractions.AbstractAspectNetAttribute::OnEntry(SimpleBitware.AspectNet.Abstractions.AspectNetEntryContext)
00A4: nop 
00A5: nop 
00A6: ldarg.0 
00A7: ldarg.1 
00A8: call System.Void ConsoleApp1.TestClassX::set_No(System.Int32)
00AD: nop 
00AE: ldarg.0 
00AF: ldarg.1 
00B0: ldc.i4.1 
00B1: add 
00B2: call System.Void ConsoleApp1.TestClassX::set_NoProt(System.Int32)
00B7: nop 
00B8: ldarg.1 
00B9: call System.Void System.Console::WriteLine(System.Int32)
00BE: nop 
00BF: ldarg.0 
00C0: ldstr X Constructor
00C5: ldarg.1 
00C6: call System.Void ConsoleApp1.TestClassX::LogMePrivate(System.String,System.Int32)
00CB: nop 
00CC: leave.s IL_00f9: nop
00CE: newobj System.Void ConsoleApp1.Log2Attribute::.ctor()
00D3: stloc.0 
00D4: leave.s IL_00f9: nop
00D6: stloc.3 
00D7: newobj System.Void SimpleBitware.AspectNet.Abstractions.AspectNetExceptionContext::.ctor()
00DC: stloc.s V_4
00DE: ldloc.s V_4
00E0: ldloc.3 
00E1: callvirt System.Void modreq(System.Runtime.CompilerServices.IsExternalInit) SimpleBitware.AspectNet.Abstractions.AspectNetExceptionContext::set_Exception(System.Exception)
00E6: ldloc.1 
00E7: ldloc.s V_4
00E9: callvirt System.Void SimpleBitware.AspectNet.Abstractions.AbstractAspectNetAttribute::OnException(SimpleBitware.AspectNet.Abstractions.AspectNetExceptionContext)
00EE: rethrow 
00F0: nop 
00F1: ldloc.1 
00F2: ldloc.2 
00F3: callvirt System.Void SimpleBitware.AspectNet.Abstractions.AbstractAspectNetAttribute::OnExit(SimpleBitware.AspectNet.Abstractions.AspectNetExitContext)
00F8: endfinally 
00F9: nop 
00FA: leave.s IL_012d: nop
00FC: newobj System.Void ConsoleApp1.LogAttribute::.ctor()
0101: stloc.s V_5
0103: leave.s IL_012d: nop
0105: stloc.s V_8
0107: newobj System.Void SimpleBitware.AspectNet.Abstractions.AspectNetExceptionContext::.ctor()
010C: stloc.s V_9
010E: ldloc.s V_9
0110: ldloc.s V_8
0112: callvirt System.Void modreq(System.Runtime.CompilerServices.IsExternalInit) SimpleBitware.AspectNet.Abstractions.AspectNetExceptionContext::set_Exception(System.Exception)
0117: ldloc.s V_6
0119: ldloc.s V_9
011B: callvirt System.Void SimpleBitware.AspectNet.Abstractions.AbstractAspectNetAttribute::OnException(SimpleBitware.AspectNet.Abstractions.AspectNetExceptionContext)
0120: rethrow 
0122: nop 
0123: ldloc.s V_6
0125: ldloc.s V_7
0127: callvirt System.Void SimpleBitware.AspectNet.Abstractions.AbstractAspectNetAttribute::OnExit(SimpleBitware.AspectNet.Abstractions.AspectNetExitContext)
012C: endfinally 
012D: nop 
012E: ret 


.method System.Void ConsoleApp1.TestClassX::LogMe(System.String,System.Int32)
0000: newobj System.Void ConsoleApp1.LogAttribute::.ctor()
0005: stloc.2 
0006: newobj System.Void SimpleBitware.AspectNet.Abstractions.AspectNetExitContext::.ctor()
000B: stloc.3 
000C: ldloc.3 
000D: ldstr ConsoleApp1.TestClassX
0012: callvirt System.Void modreq(System.Runtime.CompilerServices.IsExternalInit) SimpleBitware.AspectNet.Abstractions.AbstractAspectNetContext::set_ClassName(System.String)
0017: ldloc.3 
0018: ldstr LogMe
001D: callvirt System.Void modreq(System.Runtime.CompilerServices.IsExternalInit) SimpleBitware.AspectNet.Abstractions.AbstractAspectNetContext::set_MemberName(System.String)
0022: ldloc.3 
0023: newobj System.Void System.Collections.Generic.Dictionary`2<System.String,System.Object>::.ctor()
0028: callvirt System.Void modreq(System.Runtime.CompilerServices.IsExternalInit) SimpleBitware.AspectNet.Abstractions.AbstractAspectNetContext::set_Parameters(System.Collections.Generic.Dictionary`2<System.String,System.Object>)
002D: ldloc.3 
002E: callvirt System.Collections.Generic.Dictionary`2<System.String,System.Object> SimpleBitware.AspectNet.Abstractions.AbstractAspectNetContext::get_Parameters()
0033: ldstr message
0038: ldarg.1 
0039: callvirt System.Void System.Collections.Generic.Dictionary`2<System.String,System.Object>::Add(TKey,TValue)
003E: ldloc.3 
003F: callvirt System.Collections.Generic.Dictionary`2<System.String,System.Object> SimpleBitware.AspectNet.Abstractions.AbstractAspectNetContext::get_Parameters()
0044: ldstr no
0049: ldarg.2 
004A: box System.Int32
004F: callvirt System.Void System.Collections.Generic.Dictionary`2<System.String,System.Object>::Add(TKey,TValue)
0054: nop 
0055: ldloc.2 
0056: ldloc.3 
0057: callvirt System.Void SimpleBitware.AspectNet.Abstractions.AbstractAspectNetAttribute::OnEntry(SimpleBitware.AspectNet.Abstractions.AspectNetEntryContext)
005C: nop 
005D: ldloca.s V_0
005F: ldc.i4.8 
0060: ldc.i4.2 
0061: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::.ctor(System.Int32,System.Int32)
0066: ldloca.s V_0
0068: ldstr LogMe: 
006D: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::AppendLiteral(System.String)
0072: nop 
0073: ldloca.s V_0
0075: ldarg.1 
0076: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::AppendFormatted(System.String)
007B: nop 
007C: ldloca.s V_0
007E: ldstr  
0083: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::AppendLiteral(System.String)
0088: nop 
0089: ldloca.s V_0
008B: ldarg.2 
008C: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::AppendFormatted<System.Int32>(!!0)
0091: nop 
0092: ldloca.s V_0
0094: call System.String System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::ToStringAndClear()
0099: call System.Void System.Console::WriteLine(System.String)
009E: nop 
009F: leave.s IL_00ce: nop
00A1: newobj System.Void ConsoleApp1.LogAttribute::.ctor()
00A6: stloc.1 
00A7: leave.s IL_00ce: nop
00A9: stloc.s V_4
00AB: newobj System.Void SimpleBitware.AspectNet.Abstractions.AspectNetExceptionContext::.ctor()
00B0: stloc.s V_5
00B2: ldloc.s V_5
00B4: ldloc.s V_4
00B6: callvirt System.Void modreq(System.Runtime.CompilerServices.IsExternalInit) SimpleBitware.AspectNet.Abstractions.AspectNetExceptionContext::set_Exception(System.Exception)
00BB: ldloc.2 
00BC: ldloc.s V_5
00BE: callvirt System.Void SimpleBitware.AspectNet.Abstractions.AbstractAspectNetAttribute::OnException(SimpleBitware.AspectNet.Abstractions.AspectNetExceptionContext)
00C3: rethrow 
00C5: nop 
00C6: ldloc.2 
00C7: ldloc.3 
00C8: callvirt System.Void SimpleBitware.AspectNet.Abstractions.AbstractAspectNetAttribute::OnExit(SimpleBitware.AspectNet.Abstractions.AspectNetExitContext)
00CD: endfinally 
00CE: nop 
00CF: ret 


.method System.Void ConsoleApp1.TestClassX::LogMePrivate(System.String,System.Int32)
0000: newobj System.Void ConsoleApp1.LogAttribute::.ctor()
0005: stloc.2 
0006: newobj System.Void SimpleBitware.AspectNet.Abstractions.AspectNetExitContext::.ctor()
000B: stloc.3 
000C: ldloc.3 
000D: ldstr ConsoleApp1.TestClassX
0012: callvirt System.Void modreq(System.Runtime.CompilerServices.IsExternalInit) SimpleBitware.AspectNet.Abstractions.AbstractAspectNetContext::set_ClassName(System.String)
0017: ldloc.3 
0018: ldstr LogMePrivate
001D: callvirt System.Void modreq(System.Runtime.CompilerServices.IsExternalInit) SimpleBitware.AspectNet.Abstractions.AbstractAspectNetContext::set_MemberName(System.String)
0022: ldloc.3 
0023: newobj System.Void System.Collections.Generic.Dictionary`2<System.String,System.Object>::.ctor()
0028: callvirt System.Void modreq(System.Runtime.CompilerServices.IsExternalInit) SimpleBitware.AspectNet.Abstractions.AbstractAspectNetContext::set_Parameters(System.Collections.Generic.Dictionary`2<System.String,System.Object>)
002D: ldloc.3 
002E: callvirt System.Collections.Generic.Dictionary`2<System.String,System.Object> SimpleBitware.AspectNet.Abstractions.AbstractAspectNetContext::get_Parameters()
0033: ldstr message
0038: ldarg.1 
0039: callvirt System.Void System.Collections.Generic.Dictionary`2<System.String,System.Object>::Add(TKey,TValue)
003E: ldloc.3 
003F: callvirt System.Collections.Generic.Dictionary`2<System.String,System.Object> SimpleBitware.AspectNet.Abstractions.AbstractAspectNetContext::get_Parameters()
0044: ldstr no
0049: ldarg.2 
004A: box System.Int32
004F: callvirt System.Void System.Collections.Generic.Dictionary`2<System.String,System.Object>::Add(TKey,TValue)
0054: nop 
0055: ldloc.2 
0056: ldloc.3 
0057: callvirt System.Void SimpleBitware.AspectNet.Abstractions.AbstractAspectNetAttribute::OnEntry(SimpleBitware.AspectNet.Abstractions.AspectNetEntryContext)
005C: nop 
005D: ldloca.s V_0
005F: ldc.i4.s 16
0061: ldc.i4.2 
0062: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::.ctor(System.Int32,System.Int32)
0067: ldloca.s V_0
0069: ldstr LogMe Private: 
006E: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::AppendLiteral(System.String)
0073: nop 
0074: ldloca.s V_0
0076: ldarg.1 
0077: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::AppendFormatted(System.String)
007C: nop 
007D: ldloca.s V_0
007F: ldstr  
0084: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::AppendLiteral(System.String)
0089: nop 
008A: ldloca.s V_0
008C: ldarg.2 
008D: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::AppendFormatted<System.Int32>(!!0)
0092: nop 
0093: ldloca.s V_0
0095: call System.String System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::ToStringAndClear()
009A: call System.Void System.Console::WriteLine(System.String)
009F: nop 
00A0: leave.s IL_00cf: nop
00A2: newobj System.Void ConsoleApp1.LogAttribute::.ctor()
00A7: stloc.1 
00A8: leave.s IL_00cf: nop
00AA: stloc.s V_4
00AC: newobj System.Void SimpleBitware.AspectNet.Abstractions.AspectNetExceptionContext::.ctor()
00B1: stloc.s V_5
00B3: ldloc.s V_5
00B5: ldloc.s V_4
00B7: callvirt System.Void modreq(System.Runtime.CompilerServices.IsExternalInit) SimpleBitware.AspectNet.Abstractions.AspectNetExceptionContext::set_Exception(System.Exception)
00BC: ldloc.2 
00BD: ldloc.s V_5
00BF: callvirt System.Void SimpleBitware.AspectNet.Abstractions.AbstractAspectNetAttribute::OnException(SimpleBitware.AspectNet.Abstractions.AspectNetExceptionContext)
00C4: rethrow 
00C6: nop 
00C7: ldloc.2 
00C8: ldloc.3 
00C9: callvirt System.Void SimpleBitware.AspectNet.Abstractions.AbstractAspectNetAttribute::OnExit(SimpleBitware.AspectNet.Abstractions.AspectNetExitContext)
00CE: endfinally 
00CF: nop 
00D0: ret 


.method System.Threading.Tasks.Task ConsoleApp1.TestClassX::LogMe(System.String)
0000: newobj System.Void ConsoleApp1.LogAttribute::.ctor()
0005: stloc.2 
0006: newobj System.Void SimpleBitware.AspectNet.Abstractions.AspectNetExitContext::.ctor()
000B: stloc.3 
000C: ldloc.3 
000D: ldstr ConsoleApp1.TestClassX
0012: callvirt System.Void modreq(System.Runtime.CompilerServices.IsExternalInit) SimpleBitware.AspectNet.Abstractions.AbstractAspectNetContext::set_ClassName(System.String)
0017: ldloc.3 
0018: ldstr LogMe
001D: callvirt System.Void modreq(System.Runtime.CompilerServices.IsExternalInit) SimpleBitware.AspectNet.Abstractions.AbstractAspectNetContext::set_MemberName(System.String)
0022: ldloc.3 
0023: newobj System.Void System.Collections.Generic.Dictionary`2<System.String,System.Object>::.ctor()
0028: callvirt System.Void modreq(System.Runtime.CompilerServices.IsExternalInit) SimpleBitware.AspectNet.Abstractions.AbstractAspectNetContext::set_Parameters(System.Collections.Generic.Dictionary`2<System.String,System.Object>)
002D: ldloc.3 
002E: callvirt System.Collections.Generic.Dictionary`2<System.String,System.Object> SimpleBitware.AspectNet.Abstractions.AbstractAspectNetContext::get_Parameters()
0033: ldstr message
0038: ldarg.0 
0039: callvirt System.Void System.Collections.Generic.Dictionary`2<System.String,System.Object>::Add(TKey,TValue)
003E: nop 
003F: ldloc.2 
0040: ldloc.3 
0041: callvirt System.Void SimpleBitware.AspectNet.Abstractions.AbstractAspectNetAttribute::OnEntry(SimpleBitware.AspectNet.Abstractions.AspectNetEntryContext)
0046: newobj System.Void ConsoleApp1.TestClassX/<LogMe>d__3::.ctor()
004B: stloc.0 
004C: ldloc.0 
004D: call System.Runtime.CompilerServices.AsyncTaskMethodBuilder System.Runtime.CompilerServices.AsyncTaskMethodBuilder::Create()
0052: stfld System.Runtime.CompilerServices.AsyncTaskMethodBuilder ConsoleApp1.TestClassX/<LogMe>d__3::<>t__builder
0057: ldloc.0 
0058: ldarg.0 
0059: stfld System.String ConsoleApp1.TestClassX/<LogMe>d__3::message
005E: ldloc.0 
005F: ldc.i4.m1 
0060: stfld System.Int32 ConsoleApp1.TestClassX/<LogMe>d__3::<>1__state
0065: ldloc.0 
0066: ldflda System.Runtime.CompilerServices.AsyncTaskMethodBuilder ConsoleApp1.TestClassX/<LogMe>d__3::<>t__builder
006B: ldloca.s V_0
006D: call System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder::Start<ConsoleApp1.TestClassX/<LogMe>d__3>(!!0&)
0072: ldloc.0 
0073: ldflda System.Runtime.CompilerServices.AsyncTaskMethodBuilder ConsoleApp1.TestClassX/<LogMe>d__3::<>t__builder
0078: call System.Threading.Tasks.Task System.Runtime.CompilerServices.AsyncTaskMethodBuilder::get_Task()
007D: stloc.s V_5
007F: leave.s IL_00b6: nop
0081: newobj System.Void ConsoleApp1.LogAttribute::.ctor()
0086: stloc.1 
0087: leave.s IL_00b6: nop
0089: stloc.s V_4
008B: newobj System.Void SimpleBitware.AspectNet.Abstractions.AspectNetExceptionContext::.ctor()
0090: stloc.s V_6
0092: ldloc.s V_6
0094: ldloc.s V_4
0096: callvirt System.Void modreq(System.Runtime.CompilerServices.IsExternalInit) SimpleBitware.AspectNet.Abstractions.AspectNetExceptionContext::set_Exception(System.Exception)
009B: ldloc.2 
009C: ldloc.s V_6
009E: callvirt System.Void SimpleBitware.AspectNet.Abstractions.AbstractAspectNetAttribute::OnException(SimpleBitware.AspectNet.Abstractions.AspectNetExceptionContext)
00A3: rethrow 
00A5: nop 
00A6: ldloc.3 
00A7: ldloc.s V_5
00A9: callvirt System.Void modreq(System.Runtime.CompilerServices.IsExternalInit) SimpleBitware.AspectNet.Abstractions.AspectNetExitContext::set_ReturnValue(System.Object)
00AE: ldloc.2 
00AF: ldloc.3 
00B0: callvirt System.Void SimpleBitware.AspectNet.Abstractions.AbstractAspectNetAttribute::OnExit(SimpleBitware.AspectNet.Abstractions.AspectNetExitContext)
00B5: endfinally 
00B6: nop 
00B7: ldloc.s V_5
00B9: ret 


.method System.Int32 ConsoleApp1.TestClassX::get_No()
0000: ldarg.0 
0001: ldfld System.Int32 ConsoleApp1.TestClassX::<No>k__BackingField
0006: ret 


.method System.Void ConsoleApp1.TestClassX::set_No(System.Int32)
0000: ldarg.0 
0001: ldarg.1 
0002: stfld System.Int32 ConsoleApp1.TestClassX::<No>k__BackingField
0007: ret 


.method System.Int32 ConsoleApp1.TestClassX::get_NoProt()
0000: ldarg.0 
0001: ldfld System.Int32 ConsoleApp1.TestClassX::<NoProt>k__BackingField
0006: ret 


.method System.Void ConsoleApp1.TestClassX::set_NoProt(System.Int32)
0000: ldarg.0 
0001: ldarg.1 
0002: stfld System.Int32 ConsoleApp1.TestClassX::<NoProt>k__BackingField
0007: ret 

