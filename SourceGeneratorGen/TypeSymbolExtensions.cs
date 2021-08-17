using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace SourceGeneratorGen
{
    public static class TypeSymbolExtensions
    {
        public static bool TypeSymbolMatchesType(this ITypeSymbol typeSymbol, Type type, SemanticModel semanticModel)
        {
            return GetTypeSymbolForType(type, semanticModel).Equals(typeSymbol);
        }

        private static INamedTypeSymbol GetTypeSymbolForType(Type type, SemanticModel semanticModel)
        {
            if (!type.IsConstructedGenericType)
            {
                return semanticModel.Compilation.GetTypeByMetadataName(type.FullName!);
            }

            // get all typeInfo's for the Type arguments 
            var typeArgumentsTypeInfos = type.GenericTypeArguments.Select(a => GetTypeSymbolForType(a, semanticModel));

            var openType = type.GetGenericTypeDefinition();
            var typeSymbol = semanticModel.Compilation.GetTypeByMetadataName(openType.FullName!);
            return typeSymbol?.Construct(typeArgumentsTypeInfos.ToArray<ITypeSymbol>());
        }
    }
}