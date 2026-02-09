using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using SimpleBitware.AspectNet.Common;

namespace SimpleBitware.AspectNet.CSharp;

public class CSharpCodeEmitter : ICodeEmitter
{
public SourceFile? Emit(
    INamedTypeSymbol namedTypeSymbol,
    IReadOnlyList<SemanticWeavingPlan> plans)
{
    if (plans.Count == 0)
        return null;

    var firstPlan = plans[0];

    // Get the full syntax tree root (preserves usings, namespace, trivia)
    var root = firstPlan.Candidate.SyntaxNode.SyntaxTree.GetCompilationUnitRoot();

    // Get the parent type declaration
    if (firstPlan.Candidate.SyntaxNode.Parent is not TypeDeclarationSyntax parent)
        return null;

    // Build a map of original → rewritten members
    var rewrittenMap = new Dictionary<MemberDeclarationSyntax, MemberDeclarationSyntax>();

    foreach (var plan in plans)
    {
        if (plan.Candidate.SyntaxNode is not MemberDeclarationSyntax originalMember)
            continue;

        var rewritten = RewriteMember(
            originalMember,
            plan.Candidate.SemanticModel,
            plan);

        // Preserve trivia (comments, attributes, whitespace)
        rewritten = rewritten
            .WithLeadingTrivia(originalMember.GetLeadingTrivia())
            .WithTrailingTrivia(originalMember.GetTrailingTrivia());

        rewrittenMap[originalMember] = rewritten;
    }

    // Rebuild the class with:
    // - rewritten members for decorated ones
    // - original members for everything else
    var newMembers = new List<MemberDeclarationSyntax>();

    foreach (var member in parent.Members)
    {
        if (rewrittenMap.TryGetValue(member, out var rewritten))
            newMembers.Add(rewritten);
        else
            newMembers.Add(member); // keep original
    }

    var newParent = parent.WithMembers(SyntaxFactory.List(newMembers));

    // Replace the class in the root
    var newRoot = root.ReplaceNode(parent, newParent);

    // Emit full file (usings + namespace + rewritten class)
    var sourceString = newRoot.NormalizeWhitespace().ToFullString();
    var sourceText = SourceText.From(sourceString, Encoding.UTF8);

    var fileName = GetGeneratedFileName(namedTypeSymbol.Name);
    return new SourceFile(fileName, sourceText);
}




    private static string GetGeneratedFileName(string className)
    {
        return $"{className}.g.cs";
    }
    
    private static MemberDeclarationSyntax RewriteMember(
        MemberDeclarationSyntax syntax,
        SemanticModel model,
        SemanticWeavingPlan plan)
    {
        return syntax switch
        {
            MethodDeclarationSyntax m => RewriteMethod(m, model, plan),
            // PropertyDeclarationSyntax p => RewriteProperty(p, model, plan),
            _ => syntax
        };
    }
    
    private static MethodDeclarationSyntax RewriteMethod(
        MethodDeclarationSyntax syntax,
        SemanticModel model,
        SemanticWeavingPlan plan)
    {
        // Extract original body
        BlockSyntax body = syntax.Body;

        if (body == null && syntax.ExpressionBody != null)
        {
            body = SyntaxFactory.Block(
                SyntaxFactory.ReturnStatement(syntax.ExpressionBody.Expression));
        }

        if (body == null)
            return syntax; // abstract, extern, interface

        // Remove AspectNet attributes
        var cleaned = RemoveAspectAttributes(syntax, model, plan);

        // Apply weaving for each aspect
        BlockSyntax woven = body;

        foreach (var aspect in plan.Aspects)
            woven = WrapWithTryCatch(woven, aspect);

        // Replace body
        return cleaned
            .WithBody(woven)
            .WithExpressionBody(null)
            .WithSemicolonToken(default);
    }
    
    private static MethodDeclarationSyntax RemoveAspectAttributes(
        MethodDeclarationSyntax syntax,
        SemanticModel model,
        SemanticWeavingPlan plan)
    {
        var cleanedLists = new List<AttributeListSyntax>();

        foreach (var list in syntax.AttributeLists)
        {
            var kept = list.Attributes
                .Where(a => !plan.Aspects.Any(x =>
                    SymbolEqualityComparer.Default.Equals(
                        model.GetSymbolInfo(a).Symbol?.ContainingType,
                        x.Attribute.AttributeClass)))
                .ToList();

            if (kept.Count > 0)
                cleanedLists.Add(
                    SyntaxFactory.AttributeList(
                        SyntaxFactory.SeparatedList(kept)));
        }

        return syntax.WithAttributeLists(SyntaxFactory.List(cleanedLists));
    }

    
    
    
    
    
    



    private static SourceText EmitPartialClass(
        INamedTypeSymbol namedTypeSymbol,
        IReadOnlyList<SemanticWeavingPlan> semanticWeavingPlans)
    {
        var sb = new StringBuilder(4096);

        // Namespace
        if (!namedTypeSymbol.ContainingNamespace.IsGlobalNamespace)
        {
            sb.Append("namespace ")
                .Append(namedTypeSymbol.ContainingNamespace.ToDisplayString())
                .AppendLine(";");
            sb.AppendLine();
        }

        // Type header — preserve all modifiers from the original symbol
        // (class/struct/record, accessibility, static, abstract, sealed, etc.)
        {
            var declaration = namedTypeSymbol.ToDisplayString(
                SymbolDisplayFormat.MinimallyQualifiedFormat
                    .WithMemberOptions(SymbolDisplayMemberOptions.IncludeModifiers)
                    .WithKindOptions(SymbolDisplayKindOptions.IncludeTypeKeyword));

            // We only want the header, not the namespace-qualified name
            // e.g. "public partial class Foo" instead of "MyNs.Foo"
            var lastDot = declaration.LastIndexOf('.');
            if (lastDot >= 0)
                declaration = declaration.Substring(lastDot + 1);

            // Ensure "partial" is present
            if (!declaration.Contains("partial"))
                declaration = declaration.Replace("class", "partial class");

            sb.AppendLine(declaration);
        }

        sb.AppendLine("{");

        foreach (var plan in semanticWeavingPlans)
        {
            EmitMember(sb, plan);
            sb.AppendLine();
        }

        sb.AppendLine("}");

        return SourceText.From(sb.ToString(), Encoding.UTF8);
    }

    private static void EmitMember(StringBuilder sb, SemanticWeavingPlan semanticWeavingPlan)
    {
        switch (semanticWeavingPlan.Candidate.SyntaxNode)
        {
            case MethodDeclarationSyntax methodSyntax:
                EmitMethod(sb, methodSyntax, semanticWeavingPlan);
                break;

            // case PropertyDeclarationSyntax propertySyntax:
            //     EmitProperty(sb, propertySyntax, semanticModel, plan);
            //     break;
            //
            // case FieldDeclarationSyntax fieldSyntax:
            //     EmitField(sb, fieldSyntax, semanticModel, plan);
            //     break;
            //
            // case EventDeclarationSyntax eventSyntax:
            //     EmitEvent(sb, eventSyntax, semanticModel, plan);
            //     break;

            default:
                // Unsupported member type — skip
                break;
        }
    }

    private static void EmitMethod(
        StringBuilder sb,
        MethodDeclarationSyntax syntax,
        SemanticWeavingPlan semanticWeavingPlan)
    {
        var semanticModel = semanticWeavingPlan.Candidate.SemanticModel;

        // ---------------------------------------------------------------------
        // 1. Extract original method body (block or expression-bodied)
        // ---------------------------------------------------------------------
        BlockSyntax? body = syntax.Body;

        if (body == null && syntax.ExpressionBody != null)
        {
            body = SyntaxFactory.Block(
                SyntaxFactory.ReturnStatement(syntax.ExpressionBody.Expression));
        }

        if (body == null)
            return; // abstract, extern, interface, etc.

        // ---------------------------------------------------------------------
        // 2. Remove all AspectNet attributes (in order)
        // ---------------------------------------------------------------------
        var cleanedAttributeLists = new List<AttributeListSyntax>();

        foreach (var list in syntax.AttributeLists)
        {
            var kept = list.Attributes
                .Where(a => !semanticWeavingPlan.Aspects.Any(x =>
                    SymbolEqualityComparer.Default.Equals(
                        semanticModel.GetSymbolInfo(a).Symbol?.ContainingType,
                        x.Attribute.AttributeClass)))
                .ToList();

            if (kept.Count > 0)
                cleanedAttributeLists.Add(
                    SyntaxFactory.AttributeList(SyntaxFactory.SeparatedList(kept)));
        }

        syntax = syntax.WithAttributeLists(SyntaxFactory.List(cleanedAttributeLists));

        // ---------------------------------------------------------------------
        // 3. Apply weaving for each aspect in declaration order
        // ---------------------------------------------------------------------
        BlockSyntax wovenBody = body;

        foreach (var aspect in semanticWeavingPlan.Aspects)
        {
            wovenBody = WrapWithTryCatch(wovenBody, aspect);
        }

        // ---------------------------------------------------------------------
        // 4. Reconstruct the method with the final woven body
        // ---------------------------------------------------------------------
        var rewritten = syntax
            .WithBody(wovenBody)
            .WithExpressionBody(null)
            .WithSemicolonToken(default);

        // ---------------------------------------------------------------------
        // 5. Emit the rewritten method
        // ---------------------------------------------------------------------
        sb.AppendLine(rewritten.NormalizeWhitespace().ToFullString());
    }

    private static BlockSyntax WrapWithTryCatch(BlockSyntax innerBody, AspectInstance aspect)
    {
        // ---------------------------------------------------------------------
        // Build: aspect.OnEntry();
        // ---------------------------------------------------------------------
        var onEntryStatement =
            SyntaxFactory.ExpressionStatement(
                SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName(aspect.InstanceName),
                        SyntaxFactory.IdentifierName("OnEntry"))));

        // ---------------------------------------------------------------------
        // Build: aspect.OnExit();
        // ---------------------------------------------------------------------
        var onExitStatement =
            SyntaxFactory.ExpressionStatement(
                SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName(aspect.InstanceName),
                        SyntaxFactory.IdentifierName("OnExit"))));

        // ---------------------------------------------------------------------
        // Build: aspect.OnException(ex);
        // ---------------------------------------------------------------------
        var onExceptionStatement =
            SyntaxFactory.ExpressionStatement(
                SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName(aspect.InstanceName),
                        SyntaxFactory.IdentifierName("OnException")),
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.Argument(
                                SyntaxFactory.IdentifierName("ex"))))));

        // ---------------------------------------------------------------------
        // Build try { <innerBody> }
        // ---------------------------------------------------------------------
        var tryBlock = SyntaxFactory.Block(innerBody.Statements);

        // ---------------------------------------------------------------------
        // Build catch (Exception ex) { aspect.OnException(ex); throw; }
        // ---------------------------------------------------------------------
        var catchClause =
            SyntaxFactory.CatchClause()
                .WithDeclaration(
                    SyntaxFactory.CatchDeclaration(
                        SyntaxFactory.ParseTypeName("Exception"),
                        SyntaxFactory.Identifier("ex")))
                .WithBlock(
                    SyntaxFactory.Block(
                        onExceptionStatement,
                        SyntaxFactory.ThrowStatement()));

        // ---------------------------------------------------------------------
        // Build finally { aspect.OnExit(); }
        // ---------------------------------------------------------------------
        var finallyClause =
            SyntaxFactory.FinallyClause(
                SyntaxFactory.Block(onExitStatement));

        // ---------------------------------------------------------------------
        // Final structure:
        //
        // {
        //     aspect.OnEntry();
        //     try {
        //         <innerBody>
        //     }
        //     catch (Exception ex) {
        //         aspect.OnException(ex);
        //         throw;
        //     }
        //     finally {
        //         aspect.OnExit();
        //     }
        // }
        // ---------------------------------------------------------------------
        return SyntaxFactory.Block(
            onEntryStatement,
            SyntaxFactory.TryStatement(
                tryBlock,
                SyntaxFactory.SingletonList(catchClause),
                finallyClause));
    }


    private static void EmitProperty(
        StringBuilder sb,
        IPropertySymbol prop,
        SemanticWeavingPlan semanticWeavingPlan)
    {
        sb.Append("    // Property weaving not implemented yet: ")
            .AppendLine(prop.Name);
    }

    private static void EmitField(
        StringBuilder sb,
        IFieldSymbol field,
        SemanticWeavingPlan semanticWeavingPlan)
    {
        sb.Append("    // Field weaving not implemented yet: ")
            .AppendLine(field.Name);
    }

    private static void EmitEvent(
        StringBuilder sb,
        IEventSymbol evt,
        SemanticWeavingPlan semanticWeavingPlan)
    {
        sb.Append("    // Event weaving not implemented yet: ")
            .AppendLine(evt.Name);
    }
}
