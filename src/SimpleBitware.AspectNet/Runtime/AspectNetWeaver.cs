using Microsoft.Build.Utilities;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using SimpleBitware.AspectNet.Abstractions;
using SimpleBitware.AspectNet.Extensions;
using SimpleBitware.AspectNet.Extensions.Cecil;

namespace SimpleBitware.AspectNet.Runtime;

public static class AspectNetWeaver
{
    public static void Run(string assemblyPath, TaskLoggingHelper log)
    {
        var pdbFilePath = GetPdbFilePath(assemblyPath);
        var targetAssemblyDirectory = GetTargetAssemblyDirectory(assemblyPath)!;
        var readerParams = GetReaderParameters(targetAssemblyDirectory, pdbFilePath);
        
        ProcessModule(assemblyPath, pdbFilePath, readerParams);
    }

    private static string? GetPdbFilePath(string assemblyPath) => Path.ChangeExtension(assemblyPath, "pdb");

    private static string? GetTargetAssemblyDirectory(string assemblyPath) => Path.GetDirectoryName(assemblyPath);

    private static ReaderParameters GetReaderParameters(string targetAssemblyPath, string? pdbFilePath)
    {
        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(targetAssemblyPath);

        return new ReaderParameters
        {
            ReadSymbols = File.Exists(pdbFilePath),
            ReadWrite = false,
            AssemblyResolver = resolver
        };
    }

    private static void ProcessModule(string assemblyPath, string? pdbFilePath, ReaderParameters readerParams)
    {
        Console.WriteLine($"Processing module {assemblyPath}");

        using var assemblyStream = new MemoryStream();
        using var pdbStream = new MemoryStream();
        using (var module = ModuleDefinition.ReadModule(assemblyPath, readerParams))
        {
            var baseAspectAttribute = module
                .ImportReference(typeof(AbstractAspectNetAttribute))
                .Resolve();

            foreach (var type in module.Types)
            {
                ProcessType(module, type, baseAspectAttribute);
            }

            var writerParams = new WriterParameters
            {
                WriteSymbols = readerParams.ReadSymbols,
                SymbolStream = pdbStream,
                SymbolWriterProvider = new PortablePdbWriterProvider()
            };

            module.Write(assemblyStream, writerParams);
        }

        assemblyStream.SaveToFile(assemblyPath);
        pdbStream.SaveToFile(pdbFilePath);

        Console.WriteLine($"Module weaving complete.");
    }

    private static void ProcessType(ModuleDefinition module, TypeDefinition type, TypeDefinition baseAspectAttribute)
    {
        foreach (var method in type.Methods.Where(m => m.HasBody && !m.IsConstructor))
        {
            var aspectAttrs = method.CustomAttributes
                .Where(a => a.AttributeType.Resolve().InheritsFrom(baseAspectAttribute))
                .ToList();

            if (aspectAttrs.Count > 0)
                WeaveMethod(module, method, aspectAttrs);
        }
    }

    private static void WeaveMethod(
        ModuleDefinition module,
        MethodDefinition method,
        List<CustomAttribute> aspects)
    {
        var body = method.Body;
        body.SimplifyMacros();
        method.DebugInformation?.SequencePoints.Clear();

        var il = body.GetILProcessor();

        // remove attributes
        foreach (var a in aspects)
            method.CustomAttributes.Remove(a);

        // only one aspect for now
        var aspectAttr = aspects[0];
        var aspectTypeRef = module.ImportReference(aspectAttr.AttributeType);

        // local: aspect
        var aspectVariableDefinition = new VariableDefinition(aspectTypeRef);
        body.Variables.Add(aspectVariableDefinition);
        body.InitLocals = true;

        // stash original instructions
        var originalInstructions = body.Instructions.ToList();

        // reset body
        body.Instructions.Clear();
        body.ExceptionHandlers.Clear();

        // import AspectNetDependencyInjection.GetRequiredService<T>()
        var diType = module.ImportReference(
            typeof(AspectNetDependencyInjection));
        var diResolved = diType.Resolve();

        var getRequiredServiceGeneric = diResolved.Methods
            .First(m => m.Name == "GetRequiredService" && m.HasGenericParameters);

        var getRequiredServiceClosed = new GenericInstanceMethod(module.ImportReference(getRequiredServiceGeneric));
        getRequiredServiceClosed.GenericArguments.Add(aspectTypeRef);

        // resolve base AspectNet attribute methods
        var onEntry = module.FindMethod(aspectTypeRef, nameof(AbstractAspectNetAttribute.OnEntry), 1);
        var onExit = module.FindMethod(aspectTypeRef, nameof(AbstractAspectNetAttribute.OnExit), 1);
        var onException = module.FindMethod(aspectTypeRef, nameof(AbstractAspectNetAttribute.OnException), 2);

        // aspect = AspectNetDependencyInjection.GetRequiredService<LogAttribute>();
        il.Append(Instruction.Create(OpCodes.Call, getRequiredServiceClosed));
        il.Append(Instruction.Create(OpCodes.Stloc, aspectVariableDefinition));

        //method.WeaveTryCatchFinallyAroundBody(aspectVariableDefinition, onEntry, onException, onExit, originalInstructions);
        method.WeaveWithContextAndReturn(aspectVariableDefinition, onEntry, onException, onExit, originalInstructions);
        
        body.OptimizeMacros();
    }
}
