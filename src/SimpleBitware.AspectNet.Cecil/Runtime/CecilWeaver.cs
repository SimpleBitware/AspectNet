using System.Reflection;
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
public class CecilWeaver
{
    private static readonly Type BaseAspectNetAttributeType = typeof(IAspectNetAttribute);
    private static readonly Type ProcessedByAspectNetAttributeType = typeof(ProcessedByAspectNetAttribute);
    private static readonly Type AspectNetExcludeAttributeType = typeof(AspectNetExcludeAttribute);
    
    /// <summary>
    /// Processes an assembly.
    /// </summary>
    /// <param name="targetAssemblyDirectory">The directory containing the target assembly and its dependencies.</param>
    /// <param name="references">An array of reference assembly paths.</param>
    /// <param name="assemblyPath">The path to the assembly to process.</param>
    /// <param name="pdbFilePath">The path to the PDB file, or null if no symbols are available.</param>
    /// <param name="generateDebugFiles">Whether to generate IL output files for debugging.</param>
    /// <returns>An array of file paths for the processed assembly and PDB files.</returns>
    /// <exception cref="ApplicationException">Thrown when the module cannot be loaded.</exception>
    /// <exception cref="SymbolsNotFoundException">Thrown when required types cannot be resolved.</exception>
    /// <remarks>
    /// This generic method provides the core weaving logic.
    /// </remarks>
    public WeavingResult ProcessAssembly(
        string targetAssemblyDirectory,
        string[] references,
        string assemblyPath,
        string? pdbFilePath,
        bool generateDebugFiles)
    {
        string[] cachedItems;
        var readerParameters = GetReaderParameters(targetAssemblyDirectory, references, pdbFilePath);
        var writerParameters = GetWriteParameters(readerParameters);

        using var assemblyStream = new MemoryStream();
        using (var module = ModuleDefinition.ReadModule(assemblyPath, readerParameters))
        {
            if (module == null)
                throw new ApplicationException($"Module {assemblyPath} could not be loaded. Check the assembly path and ensure it is a valid .NET assembly.");

            if (generateDebugFiles)
                File.WriteAllText("before.il", module.DumpModule());

            var baseAspectNetAttributeTypeDefinition = module.Cache().Resolve(module.ImportReference(BaseAspectNetAttributeType));
            var aspectNetExcludeAttributeTypeReference = module.Cache().ImportReference(AspectNetExcludeAttributeType);
            var processedByAspectNetAttributeTypeReference = module.Cache().ImportReference(ProcessedByAspectNetAttributeType);

            var processedByAspectNetAttributeDefaultConstructor = ProcessedByAspectNetAttributeType.GetConstructor(Type.EmptyTypes)
                ?? throw new SymbolsNotFoundException($"Marker attribute constructor could not be resolved. Ensure that {ProcessedByAspectNetAttributeType} has a parameterless constructor.");
            var processedByAspectNetAttributeDefaultConstructorMethodReference = module.Cache().ImportReference(processedByAspectNetAttributeDefaultConstructor);

            WeaveModuleTypes(
                module.GetTypes(), 
                baseAspectNetAttributeTypeDefinition, 
                aspectNetExcludeAttributeTypeReference,
                processedByAspectNetAttributeTypeReference,
                processedByAspectNetAttributeDefaultConstructorMethodReference);

            if (generateDebugFiles)
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

    private static void WeaveModuleTypes(
        IEnumerable<TypeDefinition> moduleTypes, 
        TypeDefinition baseAspectNetAttributeTypeDefinition,
        TypeReference aspectNetExcludeAttributeTypeReference,
        TypeReference processedByAspectNetAttributeTypeReference,
        MethodReference processedByAspectNetAttributeDefaultConstructorMethodReference)
    {
        moduleTypes.ForEach(type =>
        {
            var classAspects = type.CustomAttributes.GetAspectNetDerivedAttributes(baseAspectNetAttributeTypeDefinition);
            if (classAspects.Any())
            {
                var memberToMaterialize = type.GetInheritedMembersToBridge(aspectNetExcludeAttributeTypeReference);
                type.MaterializeInheritedBridges(memberToMaterialize);
            }

            type
                .GetMethodsDecoratedWithAspectNetDerivedAttributes(
                    classAspects,
                    baseAspectNetAttributeTypeDefinition,
                    [aspectNetExcludeAttributeTypeReference, processedByAspectNetAttributeTypeReference]
                )
                .ForEach(method => method
                    .WeaveMethod()
                    .OptimizeMacros()
                    .ApplyMarkerAttribute(processedByAspectNetAttributeDefaultConstructorMethodReference)
                );
            classAspects.ForEach(attr => { type.CustomAttributes.Remove(attr); });
        });
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
