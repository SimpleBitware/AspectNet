using System.Runtime.CompilerServices;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using SimpleBitware.AspectNet.Abstractions;
using SimpleBitware.AspectNet.Extensions;
using SimpleBitware.AspectNet.Extensions.Cecil;

namespace SimpleBitware.AspectNet.Runtime.Cecil;

public static class CecilWeaver
{
    public static string[] ProcessAssembly(
        string targetAssemblyDirectory,
        string[] references,
        string assemblyPath,
        string? pdbFilePath)
    {
        return ProcessAssembly<AbstractAspectNetAttribute, AspectNetWovenAttribute>(
            targetAssemblyDirectory,
            references,
            assemblyPath,
            pdbFilePath,
            (moduleTypes, baseAspectNetAttribute, aspectNetReferences, markerAttributeConstructor) =>
                moduleTypes.GetModuleTypesWithAspectNetDerivedAttributes(
                        baseAspectNetAttribute,
                        [typeof(AspectNetExcludeAttribute), typeof(AspectNetWovenAttribute), typeof(CompilerGeneratedAttribute)]
                    )
                    .Each(x => x
                        .WeaveMethod(aspectNetReferences)
                        // .ApplyMarkerAttribute(markerAttributeConstructor)
                        // .Optimize()
                    )
        );
    }

    private static string[] ProcessAssembly<TAttribute, TMarker>(
        string targetAssemblyDirectory,
        string[] references,
        string assemblyPath,
        string? pdbFilePath,
        Action<IEnumerable<TypeDefinition>, TypeDefinition, AspectReferences, MethodReference?> weaveAction)
        where TAttribute : class
        where TMarker : class
    {
        var readerParameters = GetReaderParameters(targetAssemblyDirectory, references, pdbFilePath);
        var writerParameters = GetWriteParameters(readerParameters);

        using var assemblyStream = new MemoryStream();
        using (var module = ModuleDefinition.ReadModule(assemblyPath, readerParameters))
        {
            if (module == null)
                throw new ApplicationException($"Module {assemblyPath} could not be loaded. Check the assembly path and ensure it is a valid .NET assembly.");

            File.WriteAllText("before.il", module.DumpModuleIL());
            
            var baseAspectNetAttribute = module.ImportReference(typeof(TAttribute)).Resolve()
                ?? throw new SymbolsNotFoundException($"Base aspect attribute type could not be resolved. Ensure that the assembly references are correct and that the {nameof(TAttribute)} is accessible.");
            var aspectNetReferences = new AspectReferences(module, baseAspectNetAttribute);
            var markerAttributeConstructor = module.ImportReference(typeof(TMarker).GetConstructor(Type.EmptyTypes))
                ?? throw new SymbolsNotFoundException($"Marker attribute constructor could not be resolved. Ensure that {nameof(TMarker)} has a parameterless constructor.");

            weaveAction(module.GetTypes(), baseAspectNetAttribute, aspectNetReferences, markerAttributeConstructor);

            File.WriteAllText("after.il", module.DumpModuleIL());

            Console.WriteLine("Before module write");
            module.Write(assemblyStream, writerParameters);
        }

        return new[]
            {
                assemblyStream.SaveToFile(assemblyPath),
                writerParameters.SymbolStream?.SaveToFile(pdbFilePath)
            }
            .Where(x => x != null).ToArray()!;
    }
    
    public static string DumpModuleIL(this ModuleDefinition module)
    {
        var sb = new StringBuilder();

        foreach (var type in module.Types)
        {
            sb.AppendLine($"// Type: {type.FullName}");

            foreach (var method in type.Methods)
            {
                sb.AppendLine($"\n.method {method.FullName}");
                sb.AppendLine(method.DumpIL());
            }
        }

        return sb.ToString();
    }
    
    public static string DumpIL(this MethodDefinition method)
    {
        var sb = new StringBuilder();

        foreach (var instr in method.Body.Instructions)
        {
            sb.AppendLine($"{instr.Offset:X4}: {instr.OpCode} {instr.Operand}");
        }

        return sb.ToString();
    }



    private static ReaderParameters GetReaderParameters(string targetAssemblyDirectory, string[] references, string? pdbFilePath)
    {
        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(targetAssemblyDirectory);
        
        foreach (var reference in references)
        {
            if (File.Exists(reference))
                resolver.AddSearchDirectory(Path.GetDirectoryName(reference));
        }

        return new ReaderParameters
        {
            ReadSymbols = File.Exists(pdbFilePath),
            ReadWrite = false,
            AssemblyResolver = resolver
        };
    }

    private static WriterParameters GetWriteParameters(ReaderParameters readerParameters)
    {
        return new WriterParameters
        {
            WriteSymbols = readerParameters.ReadSymbols,
            SymbolStream = readerParameters.ReadSymbols ? new MemoryStream() : null,
            SymbolWriterProvider = new PortablePdbWriterProvider()
        };
    }

    //--------------------------------

    private static void WeaveTargetCandidate(MethodDefinition method, CustomAttribute[] aspectNetAttributes)
    {
    }

    // private static void ProcessWorkItems(Dictionary<MethodDefinition, List<CustomAttribute>> workItems, ModuleDefinition module)
    // {
    //     var aspectRefs = new AspectReferences(module);
    //     var markerCtor = module.ImportReference(typeof(AspectNetWovenAttribute).GetConstructor(Type.EmptyTypes));
    //
    //     foreach (var item in workItems)
    //     {
    //         var method = item.Key;
    //         var attributes = item.Value;
    //
    //         // 1. Sort by Priority (Ascending: 0, 10, 20...)
    //         // We use a stable sort to preserve order for same-priority attributes
    //         var sortedAspects = attributes
    //             .OrderBy(a => a.GetPriorityValue())
    //             .ToList();
    //
    //         // 2. REVERSE for Weaving
    //         // We weave the "Inner" aspects first. The "Outer" aspects will then 
    //         // wrap the code that now includes the inner aspects.
    //         sortedAspects.Reverse();
    //
    //         // Capture the original instructions as a snapshot
    //         // This prevents the weaver from accidentally wrapping its own injected IL
    //         var originalInstructions = method.Body.Instructions.ToArray();
    //
    //         foreach (var aspectAttr in sortedAspects)
    //         {
    //             // Create the local variable instance: 'new MyAspect(args) { Priority = x }'
    //             var aspectVar = method.InstantiateAspectInLocals(aspectAttr);
    //
    //             // Perform the IL Injection
    //             method.WeaveWithContextAndReturn(
    //                 aspectVar,
    //                 aspectRefs.OnEntry,
    //                 aspectRefs.OnException,
    //                 aspectRefs.OnExit,
    //                 originalInstructions
    //             );
    //
    //             // Update originalInstructions to include the newly woven layer 
    //             // if you want the NEXT aspect to wrap the previous aspect's OnEntry/OnExit.
    //             // Usually, we want all aspects to wrap the ORIGINAL code.
    //         }
    //
    //         // 3. APPLY STAMP: Mark as woven only once all attributes are processed
    //         method.CustomAttributes.Add(new CustomAttribute(markerCtor));
    //
    //         // 4. OPTIMIZE: Clean up branch offsets (Required after significant IL changes)
    //         method.Body.OptimizeMacros();
    //     }
    // }
}
