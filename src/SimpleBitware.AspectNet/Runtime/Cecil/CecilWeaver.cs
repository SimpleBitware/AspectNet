using System.Runtime.CompilerServices;
using Mono.Cecil;
using Mono.Cecil.Cil;
using SimpleBitware.AspectNet.Abstractions;
using SimpleBitware.AspectNet.Debug;
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
        return ProcessAssembly<AbstractObserverAttribute, AspectNetWovenAttribute>(
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
                        .WeaveMethod()//aspectNetReferences)
                        .ApplyMarkerAttribute(markerAttributeConstructor)
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

            //File.WriteAllText("before.il", module.DumpModuleIL());
            
            var baseAspectNetAttribute = module.ImportReference(typeof(TAttribute)).Resolve()
                ?? throw new SymbolsNotFoundException($"Base aspect attribute type could not be resolved. Ensure that the assembly references are correct and that the {nameof(TAttribute)} is accessible.");
            var aspectNetReferences = new AspectReferences(module, baseAspectNetAttribute);
            var markerAttributeConstructor = module.ImportReference(typeof(TMarker).GetConstructor(Type.EmptyTypes))
                ?? throw new SymbolsNotFoundException($"Marker attribute constructor could not be resolved. Ensure that {nameof(TMarker)} has a parameterless constructor.");

            weaveAction(module.GetTypes(), baseAspectNetAttribute, aspectNetReferences, markerAttributeConstructor);

            //File.WriteAllText("after.il", module.DumpModuleIL());

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
