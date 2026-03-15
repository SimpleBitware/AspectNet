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
0006: nop 
0007: nop 
0008: ldarg.0 
0009: ldarg.1 
000A: call System.Void ConsoleApp1.TestClassX::set_No(System.Int32)
000F: nop 
0010: ldarg.0 
0011: ldarg.1 
0012: ldc.i4.1 
0013: add 
0014: call System.Void ConsoleApp1.TestClassX::set_NoProt(System.Int32)
0019: nop 
001A: ldarg.1 
001B: call System.Void System.Console::WriteLine(System.Int32)
0020: nop 
0021: ldarg.0 
0022: ldstr X Constructor
0027: ldarg.1 
0028: call System.Void ConsoleApp1.TestClassX::LogMePrivate(System.String,System.Int32)
002D: nop 
002E: ret 


.method System.Void ConsoleApp1.TestClassX::LogMe(System.String,System.Int32)
0000: nop 
0001: ldloca.s V_0
0003: ldc.i4.8 
0004: ldc.i4.2 
0005: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::.ctor(System.Int32,System.Int32)
000A: ldloca.s V_0
000C: ldstr LogMe: 
0011: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::AppendLiteral(System.String)
0016: nop 
0017: ldloca.s V_0
0019: ldarg.1 
001A: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::AppendFormatted(System.String)
001F: nop 
0020: ldloca.s V_0
0022: ldstr  
0027: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::AppendLiteral(System.String)
002C: nop 
002D: ldloca.s V_0
002F: ldarg.2 
0030: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::AppendFormatted<System.Int32>(!!0)
0035: nop 
0036: ldloca.s V_0
0038: call System.String System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::ToStringAndClear()
003D: call System.Void System.Console::WriteLine(System.String)
0042: nop 
0043: ret 


.method System.Void ConsoleApp1.TestClassX::LogMePrivate(System.String,System.Int32)
0000: nop 
0001: ldloca.s V_0
0003: ldc.i4.s 16
0005: ldc.i4.2 
0006: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::.ctor(System.Int32,System.Int32)
000B: ldloca.s V_0
000D: ldstr LogMe Private: 
0012: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::AppendLiteral(System.String)
0017: nop 
0018: ldloca.s V_0
001A: ldarg.1 
001B: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::AppendFormatted(System.String)
0020: nop 
0021: ldloca.s V_0
0023: ldstr  
0028: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::AppendLiteral(System.String)
002D: nop 
002E: ldloca.s V_0
0030: ldarg.2 
0031: call System.Void System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::AppendFormatted<System.Int32>(!!0)
0036: nop 
0037: ldloca.s V_0
0039: call System.String System.Runtime.CompilerServices.DefaultInterpolatedStringHandler::ToStringAndClear()
003E: call System.Void System.Console::WriteLine(System.String)
0043: nop 
0044: ret 


.method System.Threading.Tasks.Task ConsoleApp1.TestClassX::LogMe(System.String)
0000: newobj System.Void ConsoleApp1.TestClassX/<LogMe>d__3::.ctor()
0005: stloc.0 
0006: ldloc.0 
0007: call System.Runtime.CompilerServices.AsyncTaskMethodBuilder System.Runtime.CompilerServices.AsyncTaskMethodBuilder::Create()
000C: stfld System.Runtime.CompilerServices.AsyncTaskMethodBuilder ConsoleApp1.TestClassX/<LogMe>d__3::<>t__builder
0011: ldloc.0 
0012: ldarg.0 
0013: stfld System.String ConsoleApp1.TestClassX/<LogMe>d__3::message
0018: ldloc.0 
0019: ldc.i4.m1 
001A: stfld System.Int32 ConsoleApp1.TestClassX/<LogMe>d__3::<>1__state
001F: ldloc.0 
0020: ldflda System.Runtime.CompilerServices.AsyncTaskMethodBuilder ConsoleApp1.TestClassX/<LogMe>d__3::<>t__builder
0025: ldloca.s V_0
0027: call System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder::Start<ConsoleApp1.TestClassX/<LogMe>d__3>(!!0&)
002C: ldloc.0 
002D: ldflda System.Runtime.CompilerServices.AsyncTaskMethodBuilder ConsoleApp1.TestClassX/<LogMe>d__3::<>t__builder
0032: call System.Threading.Tasks.Task System.Runtime.CompilerServices.AsyncTaskMethodBuilder::get_Task()
0037: ret 


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

