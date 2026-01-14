using System;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SimpleBitware.AspectNet
{
    public sealed class SourceWeaver : Task
    {
        public string ProjectDir { get; set; } = default!;
        
        public string OutDir { get; set; } = default!;

        public override bool Execute()
        {
            try
            {
                var files = Directory.GetFiles(ProjectDir, "*.cs", SearchOption.AllDirectories)
                    .Where(f => !f.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar) &&
                                !f.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar));

                foreach (var file in files)
                {
                    var text = File.ReadAllText(file);
                    var tree = CSharpSyntaxTree.ParseText(text);
                    var root = tree.GetRoot();
                    var newRoot = new LogRewriter().Visit(root);

                    var rel = Path.GetRelativePath(ProjectDir, file);
                    var dest = Path.Combine(OutDir, rel);
                    Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
                    File.WriteAllText(dest, newRoot.ToFullString());
                }

                Log.LogMessage(MessageImportance.High, $"Aspect weaving complete: {OutDir}");
                return true;
            }
            catch (Exception ex)
            {
                Log.LogErrorFromException(ex, true);
                return false;
            }
        }

        private sealed class LogRewriter : CSharpSyntaxRewriter
        {
            public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
            {
                if (!HasLog(node.AttributeLists)) return base.VisitMethodDeclaration(node);

                var methodName = node.Identifier.Text;
                var paramNames = string.Join(", ", node.ParameterList.Parameters.Select(p => p.Identifier.Text));
                var paramLog = node.ParameterList.Parameters.Select(p => LogStmt($"{p.Identifier.Text}={{" + p.Identifier.Text + "}}"));

                var normalized = NormalizeToBlock(node.ReturnType.ToString(), node.Body, node.ExpressionBody);
                var bodyStatements = normalized.Statements;

                // If method returns non-void, capture return value
                if (node.ReturnType.ToString() != "void")
                {
                    var returnVar = IdentifierName("__ret");
                    var assign = LocalDeclarationStatement(
                        VariableDeclaration(IdentifierName("var"))
                        .WithVariables(SingletonSeparatedList(
                            VariableDeclarator(Identifier("__ret"))
                            .WithInitializer(EqualsValueClause(bodyStatements.OfType<ReturnStatementSyntax>().FirstOrDefault()?.Expression ?? LiteralExpression(SyntaxKind.DefaultLiteralExpression))))));

                    var newBody = bodyStatements.Select(s => s is ReturnStatementSyntax ? ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, returnVar, ((ReturnStatementSyntax)s).Expression)) : s).ToList();
                    newBody.Insert(0, assign);
                    newBody.Add(LogStmt($"Return={{" + "__ret" + "}}"));
                    newBody.Add(ReturnStatement(returnVar));
                    bodyStatements = List(newBody);
                }

                var injected = Block(LogStmt($">>> Entering {methodName}"));
                injected = injected.AddStatements(paramLog.ToArray());
                injected = injected.AddStatements(TryStatement(Block(bodyStatements), default, FinallyClause(Block(LogStmt($"<<< Exiting {methodName}")))));

                return node.WithBody(injected).WithExpressionBody(null).WithSemicolonToken(Token(SyntaxKind.None));
            }

            public override SyntaxNode? VisitPropertyDeclaration(PropertyDeclarationSyntax node)
            {
                if (!HasLog(node.AttributeLists)) return base.VisitPropertyDeclaration(node);

                if (node.AccessorList != null && node.AccessorList.Accessors.All(a => a.Body == null && a.ExpressionBody == null))
                {
                    // Auto-property: synthesize backing field
                    var backingFieldName = "__backing_" + node.Identifier.Text;
                    var backingField = FieldDeclaration(VariableDeclaration(node.Type).WithVariables(SingletonSeparatedList(VariableDeclarator(Identifier(backingFieldName))))).WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword)));

                    var getter = AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithBody(Block(LogStmt($">>> Entering get_{node.Identifier.Text}"), ReturnStatement(IdentifierName(backingFieldName)), LogStmt($"<<< Exiting get_{node.Identifier.Text}")));
                    var setter = AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithBody(Block(LogStmt($">>> Entering set_{node.Identifier.Text}"), ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, IdentifierName(backingFieldName), IdentifierName("value"))), LogStmt($"<<< Exiting set_{node.Identifier.Text}")));

                    var newProp = node.WithAccessorList(AccessorList(List(new[] { getter, setter }))).WithInitializer(null).WithSemicolonToken(Token(SyntaxKind.None));

                    var parentClass = node.Parent as ClassDeclarationSyntax;
                    if (parentClass != null)
                    {
                        var newMembers = parentClass.Members.Insert(0, backingField);
                        var newClass = parentClass.WithMembers(newMembers);
                        return newClass;
                    }
                }
                return base.VisitPropertyDeclaration(node);
            }

            private static bool HasLog(SyntaxList<AttributeListSyntax> attrs)
            {
                return attrs.SelectMany(a => a.Attributes).Any(a => a.Name.ToString().Contains("Log"));
            }

            private static BlockSyntax NormalizeToBlock(string returnType, BlockSyntax? body, ArrowExpressionClauseSyntax? expr)
            {
                if (body != null) return body;
                var stmt = returnType == "void" ? (StatementSyntax)ExpressionStatement(expr!.Expression) : ReturnStatement(expr!.Expression);
                return Block(stmt);
            }

            private static StatementSyntax LogStmt(string text)
            {
                return ExpressionStatement(InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName("System.Console"), IdentifierName("WriteLine"))).WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(text)))))));
            }
        }
    }
}
