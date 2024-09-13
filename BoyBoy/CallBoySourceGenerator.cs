using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis;

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

            var values = context.SyntaxProvider
                .ForAttributeWithMetadataName(typeof(BoyBoyAttribute).FullName!, static (_, _) => true, Transform)
                .Where(static m => m is not null)
                .Collect();

            context.RegisterSourceOutput(values,
                static (sourceProductionContext, source) => Execute(sourceProductionContext, source!));
        }

        static void Execute(SourceProductionContext context, ImmutableArray<INamedTypeSymbol> source)
        {
            static string Show(ISymbol symbol) =>
                symbol.ToDisplayString(new SymbolDisplayFormat(
                    SymbolDisplayGlobalNamespaceStyle.Omitted,
                    SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                    SymbolDisplayGenericsOptions.IncludeTypeParameters,
                    miscellaneousOptions: SymbolDisplayMiscellaneousOptions.ExpandNullable
                ));

            foreach (var symbol in source)
            {
                var output =
                    $$"""
                      using FakeItEasy;

                      namespace BoyBoyFiles;

                      public static class {{symbol.Name}}Ex 
                      {
                      """;

                foreach (var member in symbol.GetMembers())
                {
                    if (member is not IMethodSymbol method) continue;

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
                          
                             public static void Call_{{member.Name}}(this {{Show(symbol)}} self, {{actionOrFunc}} call) {
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

        public INamedTypeSymbol? Transform(GeneratorAttributeSyntaxContext ctx, CancellationToken cancellationToken) =>
            ctx.SemanticModel.GetDeclaredSymbol(ctx.TargetNode) as INamedTypeSymbol;
    }
}