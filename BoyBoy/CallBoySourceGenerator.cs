using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Monkeymoto.GeneratorUtils;

namespace BoyBoy
{
    [Generator]
    public class CallBoySourceGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            if (!Debugger.IsAttached)
            {
                //Debugger.Launch();
            }

            var symbolTree = GenericSymbolReferenceTree.FromIncrementalGeneratorInitializationContext(context);

            var symbolToFind = context.SyntaxProvider.CreateSyntaxProvider(
                (x, _) => x is InvocationExpressionSyntax,
                (x, _) => x.SemanticModel.GetSymbolInfo(x.Node).Symbol is IMethodSymbol { Name: "Fake" } method ? method : null);

            //var symbolToFind = context.CompilationProvider.Select(static (compilation, _) =>
            //{
            //    return compilation.GetTypeByMetadataName("BoyBoy.BoyBoy")!;
            //});

            var symbols = symbolToFind
                .Combine(symbolTree)
                .SelectMany(static (x, cancellationToken) =>
                {
                    var (symbolToFind, symbolTree) = x;

                    return symbolToFind != null
                        ? symbolTree.GetBranchesBySymbol(symbolToFind, cancellationToken)
                        : [];
                })
                .Collect();

            //var values = context.SyntaxProvider
            //    .ForAttributeWithMetadataName(typeof(BoyBoyAttribute).FullName!, static (_, _) => true, Transform)
            //    .Where(static m => m is not null)
            //    .Collect();

            context.RegisterSourceOutput(symbols,
                static (sourceProductionContext, source) => Execute(sourceProductionContext, source!));
        }

        static void Execute(SourceProductionContext context, IEnumerable<GenericSymbolReference> source)
        {
            static string Show(ISymbol symbol) =>
                symbol.ToDisplayString(new SymbolDisplayFormat(
                    SymbolDisplayGlobalNamespaceStyle.Omitted,
                    SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                    SymbolDisplayGenericsOptions.IncludeTypeParameters,
                    miscellaneousOptions: SymbolDisplayMiscellaneousOptions.ExpandNullable
                ));

            var typeSymbols = source
                .Select(x => x.TypeArguments[0])
                .Distinct(SymbolEqualityComparer.Default)
                .OfType<INamedTypeSymbol>()
                .Where(x => x.TypeKind == TypeKind.Interface);

            foreach (var symbol in typeSymbols)
            {
                var output =
                    $$"""
                      using FakeItEasy;

                      namespace BoyBoyFiles;

                      public static class {{symbol.Name}}Ex 
                      {
                      """;

                foreach (var method in symbol.GetMembers().OfType<IMethodSymbol>())
                {
                    var parameters = method.Parameters;

                    var invokesOrReturns = method.ReturnsVoid ? "Invokes" : "ReturnsLazily";
                    var actionOrFunc = method.ReturnsVoid
                        ? $"System.Action<{string.Join(", ", parameters.Select(x => Show(x.Type)))}>"
                        : $"System.Func<{string.Join(", ", parameters.Select(x => Show(x.Type)))}, {Show(method.ReturnType)}>";

                    var maybeWithReturnType = method.ReturnsVoid 
                        ? ""
                        : $".WithReturnType<{Show(method.ReturnType)}>()";

                    output +=
                        $$"""
                          
                             public static void Call_{{method.Name}}(this {{Show(symbol)}} self, {{actionOrFunc}} call) {
                                 FakeItEasy.A.CallTo(self)
                                     {{maybeWithReturnType}}
                                     .WhenArgumentsMatch(collection => 
                                         {{string.Join(" && ", parameters.Select((x, i) => $"collection[{i}] is {Show(x.Type)}"))}}
                                     )
                                     .{{invokesOrReturns}}(x => call(
                                         {{string.Join(", ", parameters.Select((x, i) => $"x.Arguments.Get<{Show(x.Type)}>({i})"))}}
                                     ));
                             
                             }
                          """;
                }

                output +=
                    """
                    }
                    """;

                context.AddSource($"{symbol.Name}Ex.g.cs", output);
            }
        }
    }
}