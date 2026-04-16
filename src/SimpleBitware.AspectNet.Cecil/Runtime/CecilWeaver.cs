using Mono.Cecil;
using Mono.Cecil.Cil;
using MoreLinq;
using SimpleBitware.AspectNet.Abstractions.Attributes;
using SimpleBitware.AspectNet.Debugging;
using SimpleBitware.AspectNet.Cecil.Extensions;

namespace SimpleBitware.AspectNet.Cecil.Runtime;

public static class CecilWeaver
{
    public static string[] ProcessAssembly(
        string targetAssemblyDirectory,
        string[] references,
        string assemblyPath,
        string? pdbFilePath,
        bool generateDebugFiles)
    {
        return ProcessAssembly<IAspectNetAttribute, AspectNetWovenAttribute>(
            targetAssemblyDirectory,
            references,
            assemblyPath,
            pdbFilePath,
            (moduleTypes, baseAspectNetAttribute, markerAttributeConstructor) =>
                moduleTypes.GetMethodsDecoratedWithAspectNetDerivedAttributes(
                        baseAspectNetAttribute,
                        [typeof(AspectNetExcludeAttribute), typeof(AspectNetWovenAttribute)]
                    )
                    .ForEach(x => x
                        .WeaveMethod<AspectNetAttributeContext>()
                        .OptimizeMacros()
                        .ApplyMarkerAttribute(markerAttributeConstructor)
                    ),
            generateDebugFiles
        );
    }

    private static string[] ProcessAssembly<TAttribute, TMarker>(
        string targetAssemblyDirectory,
        string[] references,
        string assemblyPath,
        string? pdbFilePath,
        Action<IEnumerable<TypeDefinition>, TypeDefinition, MethodReference> weaveAction,
        bool generateDebugFiles)
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

            if(generateDebugFiles)
                File.WriteAllText("before.il", module.DumpModule());
            
            var baseAspectNetAttribute = module.Cache().Resolve(module.ImportReference(typeof(TAttribute)))
                ?? throw new SymbolsNotFoundException($"Base aspect attribute type could not be resolved. Ensure that the assembly references are correct and that the {nameof(TAttribute)} is accessible.");
            var markerAttributeConstructor = module.ImportReference(typeof(TMarker).GetConstructor(Type.EmptyTypes))
                ?? throw new SymbolsNotFoundException($"Marker attribute constructor could not be resolved. Ensure that {nameof(TMarker)} has a parameterless constructor.");

            weaveAction(module.GetTypes(), baseAspectNetAttribute, markerAttributeConstructor);

            if(generateDebugFiles)
                File.WriteAllText("after.il", module.DumpModule());

            module.Write(assemblyStream, writerParameters);
        }

        return new[]
            {
                assemblyStream.SaveToFile(assemblyPath),
                writerParameters.SymbolStream?.SaveToFile(pdbFilePath)
            }
            .Where(x => x != null).ToArray()!;
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
}
