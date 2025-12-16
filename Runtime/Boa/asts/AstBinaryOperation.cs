using System;
using System.Collections.Generic;

namespace _COBRA_.Boa
{
    internal class AstBinaryOperation : AstExpression
    {
        enum Depths : byte
        {
            Or,
            And,
            Equality,
            Comparison,
            Addition,
            Term,
            _last_,
        }

        enum Codes : byte
        {
            Or,
            And,
            Equal,
            NotEqual,
            Lesser,
            LesserOrEqual,
            Greater,
            GreaterOrEqual,
            Add,
            Sub,
            Multiply,
            Divide,
            Modulus,
        }

        static readonly Dictionary<string, Codes>[] codes = new Dictionary<string, Codes>[(int)Depths._last_]
        {
            new(StringComparer.OrdinalIgnoreCase)
            {
                { "or", Codes.Or },
            },
            new(StringComparer.OrdinalIgnoreCase)
            {
                { "and", Codes.And },
            },
            new(StringComparer.OrdinalIgnoreCase)
            {
                { "==", Codes.Equal },
                { "!=", Codes.NotEqual },
            },
            new(StringComparer.OrdinalIgnoreCase)
            {
                { "<", Codes.Lesser },
                { "<=", Codes.LesserOrEqual },
                { ">", Codes.Greater },
                { ">=", Codes.GreaterOrEqual },
            },
            new(StringComparer.OrdinalIgnoreCase)
            {
                { "+", Codes.Add },
                { "-", Codes.Sub },
            },
            new(StringComparer.OrdinalIgnoreCase)
            {
                { "*", Codes.Multiply },
                { "/", Codes.Divide },
                { "%", Codes.Modulus },
            },
        };

        readonly Depths depth;
        readonly Codes code;
        readonly AstExpression astL, astR;

        //----------------------------------------------------------------------------------------------------------

        AstBinaryOperation(in Depths depth, in Codes code, in AstExpression astL, in AstExpression astR) : base(typeof(bool))
        {
            this.depth = depth;
            this.code = code;
            this.astL = astL;
            this.astR = astR;
        }

        //----------------------------------------------------------------------------------------------------------

        public static bool TryOr(in CodeReader reader, in TScope tscope, in Type expected_type, out AstExpression ast_expr) => TryBinOp(reader, tscope, Depths.Or, expected_type, out ast_expr);
        static bool TryBinOp(in CodeReader reader, in TScope tscope, in Depths depth, in Type expected_type, out AstExpression ast_expr)
        {
            if (depth == Depths.Term)
            {
                if (AstUnary.TryUnary(reader, tscope, expected_type, out ast_expr))
                    while (reader.TryReadString_matches_out(out string match, false, reader.lint_theme.operators, codes[(int)depth].Keys))
                        if (AstUnary.TryUnary(reader, tscope, expected_type, out var astR))
                        {
                            Codes code = codes[(int)depth][match];
                            ast_expr = new AstBinaryOperation(depth, code, ast_expr, astR);
                            return true;
                        }
                        else
                        {
                            reader.Error($"expected expression after '{match}' operator");
                            goto failure;
                        }
                else
                    return true;
            }
            else
            {
                if (TryBinOp(reader, tscope, depth + 1, expected_type, out ast_expr))
                    while (reader.TryReadString_matches_out(out string match, false, reader.lint_theme.operators, codes[(int)depth].Keys))
                        if (TryBinOp(reader, tscope, depth + 1, expected_type, out var astR))
                        {
                            Codes code = codes[(int)depth][match];
                            ast_expr = new AstBinaryOperation(depth, code, ast_expr, astR);
                            return true;
                        }
                        else
                        {
                            reader.Error($"expected expression after '{match}' operator");
                            goto failure;
                        }
                else
                    return true;
            }

        failure:
            ast_expr = null;
            return false;
        }
    }
}