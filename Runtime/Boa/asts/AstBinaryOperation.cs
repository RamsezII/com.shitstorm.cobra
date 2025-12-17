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

        readonly Codes code;
        readonly AstExpression astL, astR;

        //----------------------------------------------------------------------------------------------------------

        AstBinaryOperation(in Codes code, in AstExpression astL, in AstExpression astR, in Type output_type) : base(output_type)
        {
            this.code = code;
            this.astL = astL;
            this.astR = astR;
        }

        //----------------------------------------------------------------------------------------------------------

        protected internal override void OnExecutorsQueue(in Janitor janitor)
        {
            base.OnExecutorsQueue(janitor);

            astL.OnExecutorsQueue(janitor);
            astR.OnExecutorsQueue(janitor);

            janitor.executors.Enqueue(new(
                name: $"op({code})",
                action_SIG_EXE: janitor =>
                {
                    var exc = new Exception($"wrong operation: {code}");

                    MemCell poppedR = janitor.vstack.PopLast();
                    MemCell poppedL = janitor.vstack.PopLast();

                    MemCell assigned = code switch
                    {
                        Codes.Or => poppedL || poppedR,
                        Codes.And => poppedL && poppedR,
                        Codes.Add => poppedL + poppedR,
                        Codes.Sub => poppedL - poppedR,
                        Codes.Equal => poppedL == poppedR,
                        Codes.NotEqual => poppedL != poppedR,
                        Codes.Lesser => poppedL < poppedR,
                        Codes.LesserOrEqual => poppedL <= poppedR,
                        Codes.Greater => poppedL > poppedR,
                        Codes.GreaterOrEqual => poppedL >= poppedR,
                        Codes.Multiply => poppedL * poppedR,
                        Codes.Divide => poppedL / poppedR,
                        Codes.Modulus => poppedL % poppedR,
                        _ => throw exc,
                    };

                    janitor.vstack.Add(assigned);
                }
            ));
        }

        //----------------------------------------------------------------------------------------------------------

        public static bool TryOr(in CodeReader reader, in MemScope tscope, in Type expected_type, out AstExpression ast_expr) => TryBinOp(reader, tscope, Depths.Or, expected_type, out ast_expr);
        static bool TryBinOp(in CodeReader reader, in MemScope tscope, in Depths depth, in Type expected_type, out AstExpression ast_expr)
        {
            if (depth == Depths.Term)
            {
                if (AstUnary.TryUnary(reader, tscope, expected_type, out ast_expr))
                {
                    while (reader.TryReadPrefixeString_matches_out(out string match, reader.lint_theme.operators, codes[(int)depth].Keys))
                        if (AstUnary.TryUnary(reader, tscope, expected_type, out var astR))
                        {
                            Codes code = codes[(int)depth][match];

                            Type output_type;
                            if (typeof(int).IsAssignableFrom(ast_expr.output_type) && (typeof(int).IsAssignableFrom(astR.output_type)))
                                output_type = typeof(int);
                            else
                                output_type = typeof(float);

                            ast_expr = new AstBinaryOperation(code, ast_expr, astR, output_type);
                        }
                        else
                        {
                            reader.CompilationError($"expected expression after '{match}' operator");
                            goto failure;
                        }
                    return true;
                }
            }
            else if (TryBinOp(reader, tscope, depth + 1, expected_type, out ast_expr))
            {
                while (reader.TryReadPrefixeString_matches_out(out string match, reader.lint_theme.operators, codes[(int)depth].Keys))
                    if (TryBinOp(reader, tscope, depth + 1, expected_type, out var astR))
                    {
                        Codes code = codes[(int)depth][match];

                        Type output_type;
                        switch (depth)
                        {
                            case Depths.Or:
                            case Depths.And:
                            case Depths.Equality:
                            case Depths.Comparison:
                                output_type = typeof(bool);
                                break;

                            case Depths.Addition:
                            case Depths.Term:
                                if (typeof(int).IsAssignableFrom(ast_expr.output_type) && (typeof(int).IsAssignableFrom(astR.output_type)))
                                    output_type = typeof(int);
                                else
                                    output_type = typeof(float);
                                break;

                            default:
                                output_type = typeof(object);
                                break;
                        }

                        ast_expr = new AstBinaryOperation(code, ast_expr, astR, output_type);
                    }
                    else
                    {
                        reader.CompilationError($"expected expression after '{match}' operator");
                        goto failure;
                    }
                return true;
            }

        failure:
            ast_expr = null;
            return false;
        }
    }
}