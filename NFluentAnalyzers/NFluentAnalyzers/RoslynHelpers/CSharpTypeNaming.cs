namespace NFluentAnalyzers.RoslynHelpers
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.CodeAnalysis;

    static class CSharpTypeNaming
    {
        public static string SpecialTypeToString(SpecialType specialType)
        {
            switch (specialType)
            {
                case SpecialType.System_Boolean:
                    return "bool";
                case SpecialType.System_Byte:
                    return "byte";
                case SpecialType.System_SByte:
                    return "sbyte";
                case SpecialType.System_Int16:
                    return "short";
                case SpecialType.System_UInt16:
                    return "ushort";
                case SpecialType.System_Int32:
                    return "int";
                case SpecialType.System_UInt32:
                    return "uint";
                case SpecialType.System_Int64:
                    return "long";
                case SpecialType.System_UInt64:
                    return "ulong";
                case SpecialType.System_Double:
                    return "double";
                case SpecialType.System_Single:
                    return "float";
                case SpecialType.System_Decimal:
                    return "decimal";
                case SpecialType.System_String:
                    return "string";
                case SpecialType.System_Char:
                    return "char";
                case SpecialType.System_Void:
                    return "void";
                case SpecialType.System_Object:
                    return "object";
                default:
                    return null;
            }
        }

        struct TypeNameVisitor
        {
            readonly StringBuilder builder;

            public TypeNameVisitor(ITypeSymbol type)
            {
                builder = new StringBuilder();

                if (type.SpecialType != SpecialType.None)
                {
                    var specialTypeString = SpecialTypeToString(type.SpecialType);
                    if (specialTypeString != null)
                    {
                        builder.Append(specialTypeString);
                        return;
                    }
                }

                Visit(type);
            }

            private void Visit(IArrayTypeSymbol type)
            {
                Visit(type.ElementType);
                builder.Append("[");
                for (int i = 1; i <= type.Rank; i++) builder.Append(",");
                builder.Append("]");
            }

            private void Visit(IPointerTypeSymbol type)
            {
                Visit(type.PointedAtType);
                builder.Append("*");
            }

            private void Visit(ITypeSymbol type)
            {
                if (type is INamedTypeSymbol)
                {
                    Visit((INamedTypeSymbol)type);
                }
                else if (type is IArrayTypeSymbol)
                {
                    Visit((IArrayTypeSymbol)type);
                }
                else if (type is IPointerTypeSymbol)
                {
                    Visit((IPointerTypeSymbol)type);
                }
                else
                {
                    throw new Exception();
                }
            }

            private void Visit(INamedTypeSymbol type)
            {
                if (type.ContainingType != null)
                {
                    Visit(type.ContainingType);
                    builder.Append("+");
                }
                else
                {
                    Visit(type.ContainingNamespace);
                }
                builder.Append(".");
                builder.Append(type.Name);
            }

            private void Visit(INamespaceSymbol ns)
            {
                if (ns.IsGlobalNamespace) return;
                if (!ns.ContainingNamespace.IsGlobalNamespace)
                {
                    Visit(ns.ContainingNamespace);
                    builder.Append(".");
                }
                builder.Append(ns.Name);
            }

            public override string ToString()
            {
                return builder.ToString();
            }
        }

        public static string GetTypeNameNoArity(ITypeSymbol typeSymbol)
        {
            return new TypeNameVisitor(typeSymbol).ToString();
        }
    }
}
