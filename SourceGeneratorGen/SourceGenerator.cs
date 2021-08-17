using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace SourceGeneratorGen
{
    public class DemoSyntaxReceiver : ISyntaxReceiver
    {
        public List<ClassDeclarationSyntax> Candidates { get; } = new();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ClassDeclarationSyntax {Identifier: {Text: "ChatHub"}} classDeclarationSyntax)
            {
                Candidates.Add(classDeclarationSyntax);
            }
        }
    }

    [Generator]
    public class JsonUsingGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is not DemoSyntaxReceiver receiver)
            {
                return;
            }

            foreach (var candidate in receiver.Candidates)
            {
                var model = context.Compilation.GetSemanticModel(candidate.SyntaxTree, true);
                var type = model.GetDeclaredSymbol(candidate) as ITypeSymbol;

                var members = type!.GetMembers()
                    .Where(symbol => symbol.DeclaredAccessibility == Accessibility.Public)
                    .Cast<IMethodSymbol>()
                    .Where(symbol => symbol.ReturnType.TypeSymbolMatchesType(typeof(Task), model))
                    .Where(symbol => !ForbiddenOperations.Contains(symbol.Name));

                var methods = members.Select(method => new HubMethod
                {
                    Name = method.Name,
                    ReturnValue = method.ReturnType.Name,
                    Parameters = method.Parameters.Select(parameter => new Parameter
                    {
                        Name = parameter.Name,
                        Type = parameter.Type.Name,
                        Namespace = parameter.Type.ContainingNamespace.ToString()
                    })
                }).ToList();

                var hubName = $"{candidate.Identifier.Text}Client";
                var generateClient = SignalrUtils.GenerateClient(hubName, methods);
                context.AddSource($"{hubName}.cs", SourceText.From(generateClient, Encoding.UTF8));
            }
        }

        private static List<string> ForbiddenOperations => new()
        {
            "get_Clients", "set_Clients", "get_Context", "set_Context", "get_Groups", "set_Groups", "OnConnectedAsync",
            "OnDisconnectedAsync", "Dispose", "CheckDisposed", "GetType", "MemberwiseClone", "Finalize", "ToString",
            "Equals", "GetHashCode"
        };
        
        // private static List<string> ForbiddenOperations { get; } = typeof(Hub).GetRuntimeMethods()
        //     .Concat(typeof(Hub<>).GetRuntimeMethods()).Select(x => x.Name).Distinct().ToList();

        public void Initialize(GeneratorInitializationContext context)
        {
#if DEBUG
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
#endif
            context.RegisterForSyntaxNotifications(() => new DemoSyntaxReceiver());
        }
    }
}