using Mono.Cecil;
using Mono.Cecil.Cil;
using MoreLinq;
using SimpleBitware.AspectNet.Abstractions.Attributes;
using SimpleBitware.AspectNet.Cecil.Debugging;
using SimpleBitware.AspectNet.Cecil.Extensions;

namespace SimpleBitware.AspectNet.Cecil.Runtime;

/// <summary>
/// Provides the main entry point for aspect weaving operations using Mono.Cecil.
/// </summary>
/// <remarks>
/// This class orchestrates the entire aspect weaving process, from loading assemblies
/// to applying aspect transformations and saving the modified assemblies.
/// </remarks>
public static class CecilWeaver
{
    /// <summary>
    /// Processes an assembly by weaving aspect attributes into the target methods.
    /// </summary>
    /// <param name="targetAssemblyDirectory">The directory containing the target assembly and its dependencies.</param>
    /// <param name="references">An array of reference assembly paths.</param>
    /// <param name="assemblyPath">The path to the assembly to process.</param>
    /// <param name="pdbFilePath">The path to the PDB file, or null if no symbols are available.</param>
    /// <param name="generateDebugFiles">Whether to generate debug output files showing before/after IL.</param>
    /// <returns>An array of file paths for the processed assembly and PDB files.</returns>
    /// <remarks>
    /// This method is the main entry point for aspect weaving. It loads the target assembly,
    /// discovers methods with aspect attributes, weaves the aspects, and saves the modified assembly.
    /// </remarks>
    public static WeavingResult ProcessAssembly(
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
                        .WeaveMethod()
                        .OptimizeMacros()
                        .ApplyMarkerAttribute(markerAttributeConstructor)
                    ),
            generateDebugFiles
        );
    }

    /// <summary>
    /// Processes an assembly with generic type parameters for aspect and marker attributes.
    /// </summary>
    /// <typeparam name="TAttribute">The base aspect attribute type.</typeparam>
    /// <typeparam name="TMarker">The marker attribute type to apply after weaving.</typeparam>
    /// <param name="targetAssemblyDirectory">The directory containing the target assembly and its dependencies.</param>
    /// <param name="references">An array of reference assembly paths.</param>
    /// <param name="assemblyPath">The path to the assembly to process.</param>
    /// <param name="pdbFilePath">The path to the PDB file, or null if no symbols are available.</param>
    /// <param name="weaveAction">The action to perform weaving on discovered methods.</param>
    /// <param name="generateDebugFiles">Whether to generate debug output files.</param>
    /// <returns>An array of file paths for the processed assembly and PDB files.</returns>
    /// <exception cref="ApplicationException">Thrown when the module cannot be loaded.</exception>
    /// <exception cref="SymbolsNotFoundException">Thrown when required types cannot be resolved.</exception>
    /// <remarks>
    /// This generic method provides the core weaving logic, allowing for different aspect
    /// and marker attribute types to be used in the weaving process.
    /// </remarks>
    private static WeavingResult ProcessAssembly<TAttribute, TMarker>(
        string targetAssemblyDirectory,
        string[] references,
        string assemblyPath,
        string? pdbFilePath,
        Action<IEnumerable<TypeDefinition>, TypeDefinition, MethodReference> weaveAction,
        bool generateDebugFiles)
        where TAttribute : class
        where TMarker : class
    {
        string[] cachedItems = [];
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
            cachedItems = module.Cache().GetCachedItems();
        }

        return new WeavingResult()
        {
            AssemblyFileName = assemblyStream.SaveToFile(assemblyPath),
            PdbFileName = writerParameters.SymbolStream?.SaveToFile(pdbFilePath),
            CachedItems = cachedItems
        };
    }
    
    /// <summary>
    /// Creates reader parameters for loading a module with symbols and assembly resolution.
    /// </summary>
    /// <param name="targetAssemblyDirectory">The directory to search for assemblies.</param>
    /// <param name="references">Additional reference assembly paths.</param>
    /// <param name="pdbFilePath">The path to the PDB file, or null.</param>
    /// <returns>The configured reader parameters.</returns>
    /// <remarks>
    /// This method sets up assembly resolution and symbol reading for the Mono.Cecil module reader.
    /// </remarks>
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

    /// <summary>
    /// Creates writer parameters for saving a module with symbols.
    /// </summary>
    /// <param name="readerParameters">The reader parameters to base the writer parameters on.</param>
    /// <returns>The configured writer parameters.</returns>
    /// <remarks>
    /// This method configures the module writer to include symbols if they were read,
    /// and sets up a memory stream for PDB data.
    /// </remarks>
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
