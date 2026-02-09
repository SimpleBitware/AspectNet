using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using SimpleBitware.AspectNet.Abstractions;
using SimpleBitware.AspectNet.Common.Debugging;
using SimpleBitware.AspectNet.Common.Extensions;

namespace SimpleBitware.AspectNet.Common;

/// <summary>
/// Discovers candidates, groups them by type, hands them to the weaver, creates the resulting source files.
/// </summary>
/// <param name="weaver">The weaver used to generate code file content.</param>
public class IncrementalCodeGenerator(IWeaver weaver) : IIncrementalGenerator
{
    private static readonly string AspectNetAttributeFullName = typeof(AspectNetAttribute).FullName!;
    private readonly IWeaver weaver = weaver ?? throw new ArgumentNullException(nameof(weaver));

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var weaveCandidates =
            context.SyntaxProvider.CreateSyntaxProvider(
                    predicate: static (_, _) => true, // semantic filtering below
                    transform: static (syntaxContext, _) =>
                    {
                        if (syntaxContext.SemanticModel.GetDeclaredSymbol(syntaxContext.Node) is not { } symbol)
                            return null;

                        var aspectBase =
                            syntaxContext.SemanticModel.Compilation.GetTypeByMetadataName(AspectNetAttributeFullName);
                        if (aspectBase is null)
                            return null;

                        var hasAspect =
                            symbol.GetAttributes()
                                .Select(attr => attr.AttributeClass)
                                .OfType<INamedTypeSymbol>()
                                .Any(attrType => attrType.InheritsFromSymbol(aspectBase));

                        return hasAspect
                            ? new WeaveCandidate(symbol, syntaxContext.Node, syntaxContext.SemanticModel)
                            : null;
                    })
                .Where(static c => c is not null)!;

        var weaveCandidateGroups =
            weaveCandidates.Select((weaveTarget, _) =>
            {
                var namedTypeSymbol = weaveTarget!.Symbol.ContainingType;
                return (Key: namedTypeSymbol, Value: weaveTarget);
            });

        var groupedWithCompilation =
            weaveCandidateGroups
                .Collect()
                .Combine(context.CompilationProvider);

        context.RegisterSourceOutput(groupedWithCompilation, (spc, tuple) =>
        {
            //DebuggerHelper.WaitForDebuggerToAttach(spc);

            var (symbols, compilation) = tuple;
            if (!symbols.Any())
                return;

            var semanticWeavingPlanner = new SemanticWeavingPlanner(compilation);
            var namedTypeSymbolsGroups = symbols.ToSymbolGroups();

            var sourceFiles = namedTypeSymbolsGroups
                .SelectMany(group =>
                    weaver.GenerateSourceFiles(
                        spc,                        // <-- pass SourceProductionContext
                        semanticWeavingPlanner,     // <-- pass planner
                        group.Key,                  // containing type
                        group.Value))               // its WeaveCandidates
                .Where(x => x is not null);

            foreach (var sourceFile in sourceFiles)
            {
                spc.AddSource(sourceFile.FileName, sourceFile.SourceText);
                spc.WriteLine($"AspectNet generated code file {sourceFile.FileName}");
            }
        });
    }
}


    
    
    // public void Initialize(IncrementalGeneratorInitializationContext context)
    // {
    //     var weaveCandidates = context.SyntaxProvider.CreateSyntaxProvider(
    //             predicate: static (_, _) => true, // allow all nodes; filtering happens semantically
    //             transform: static (syntaxContext, _) =>
    //             {
    //                 if (syntaxContext.SemanticModel.GetDeclaredSymbol(syntaxContext.Node) is not { } symbol)
    //                     return null;
    //
    //                 var aspectBase = syntaxContext.SemanticModel.Compilation.GetTypeByMetadataName(AspectNetAttributeFullName);
    //                 if (aspectBase is null)
    //                     return null;
    //
    //                 return symbol.GetAttributes()
    //                     .Select(attr => attr.AttributeClass)
    //                     .OfType<INamedTypeSymbol>()
    //                     .Any(attrType => attrType.InheritsFromSymbol(aspectBase))
    //                     ? new WeaveCandidate(
    //                         symbol,
    //                         syntaxContext.Node,
    //                         syntaxContext.SemanticModel)
    //                     : null;
    //             })
    //             .Where(static c => c is not null)!;
    //
    //     var weaveCandidateGroups = weaveCandidates
    //         .Select((weaveTarget, _) =>
    //         {
    //             var namedTypeSymbol = weaveTarget!.Symbol.ContainingType;
    //             return (Key: namedTypeSymbol, Value: weaveTarget);
    //         });
    //
    //     var combinedWeaveCandidates = weaveCandidateGroups
    //         .Collect()
    //         .Combine(context.CompilationProvider)
    //         .Select((tuple, _) =>
    //         {
    //             var (symbols, compilation) = tuple;
    //             var semanticWeavingPlanner = new SemanticWeavingPlanner(compilation);
    //             return (Symbols: symbols, SemanticWeavingPlanner: semanticWeavingPlanner);
    //         });
    //
    //     context.RegisterSourceOutput(combinedWeaveCandidates, (sourceProductionContext, combinedWeaveCandidate) =>
    //     {
    //         DebuggerHelper.WaitForDebuggerToAttach(sourceProductionContext);
    //         
    //         var (symbols, semanticWeavingPlanner) = combinedWeaveCandidate;
    //         var namedTypeSymbolsGroups = symbols.ToSymbolGroups();
    //
    //         var sourceFiles = namedTypeSymbolsGroups
    //             .SelectMany(group => weaver.GenerateSourceFiles(semanticWeavingPlanner, group.Key, group.Value));
    //
    //         foreach (var sourceFile in sourceFiles)
    //         {
    //             sourceProductionContext.AddSource(sourceFile.FileName, sourceFile.SourceText);
    //             sourceProductionContext.WriteLine($"AspectNet generated code file {sourceFile.FileName}");
    //         }
    //     });
    // }
// }
