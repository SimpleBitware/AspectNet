using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using SimpleBitware.AspectNet.Abstractions;
using SimpleBitware.AspectNet.Common.Extensions;

namespace SimpleBitware.AspectNet.Common;

/// <summary>
/// Discovers candidates, groups them by type, hands them to the weaver, creates the resulting source files.
/// </summary>
/// <param name="weaver">The weaver used to generate code file content.</param>
public class IncrementalCodeGenerator(IWeaver weaver) : IIncrementalGenerator
{
    private readonly IWeaver weaver = weaver ?? throw new ArgumentNullException(nameof(weaver));

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var weaveCandidates = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                typeof(AspectNetAttribute).FullName!,
                predicate: static (_, _) => true,
                transform: static (syntaxContext, _) =>
                    new WeaveCandidate(
                        syntaxContext.TargetSymbol,
                        syntaxContext.TargetNode,
                        syntaxContext.SemanticModel));

        var weaveCandidateGroups = weaveCandidates
            .Select((weaveTarget, _) =>
            {
                var namedTypeSymbol = weaveTarget!.Symbol.ContainingType;
                return (Key: namedTypeSymbol, Value: weaveTarget);
            });

        var combinedWeaveCandidates = weaveCandidateGroups
            .Collect()
            .Combine(context.CompilationProvider)
            .Select((tuple, _) =>
            {
                var (symbols, compilation) = tuple;
                var semanticWeavingPlanner = new SemanticWeavingPlanner(compilation);
                return (Symbols: symbols, SemanticWeavingPlanner: semanticWeavingPlanner);
            });

        context.RegisterSourceOutput(combinedWeaveCandidates, (sourceProductionContext, combinedWeaveCandidate) =>
        {
            var (symbols, semanticWeavingPlanner) = combinedWeaveCandidate;
            var namedTypeSymbolsGroups = symbols.ToSymbolGroups();

            var sourceFiles = namedTypeSymbolsGroups
                .SelectMany(group => weaver.GenerateSourceFiles(semanticWeavingPlanner, group.Key, group.Value));

            foreach (var sourceFile in sourceFiles)
            {
                sourceProductionContext.AddSource(sourceFile.FileName, sourceFile.SourceText);
            }
        });
    }
}
