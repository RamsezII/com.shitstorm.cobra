using System;
using System.Collections.Generic;

namespace _COBRA_.Boa
{
    internal class AstUnary : AstExpression
    {
        internal enum Codes : byte
        {
            Positive,
            Negative,
            Not,
            Anti,
        }

        static readonly Dictionary<string, Codes> codes = new(StringComparer.OrdinalIgnoreCase)
        {
            { "+", Codes.Positive },
            { "-", Codes.Negative },
            { "!", Codes.Not },
            { "~", Codes.Anti },
        };

        readonly Codes code;
        readonly AstExpression ast_factor;

        //----------------------------------------------------------------------------------------------------------

        protected AstUnary(in Codes code, in AstExpression ast_factor) : base(ast_factor.output_type)
        {
            this.code = code;
            this.ast_factor = ast_factor;
        }

        //----------------------------------------------------------------------------------------------------------

        public static bool TryUnary(in CodeReader reader, in TScope tscope, in Type expected_type, out AstExpression ast_unary)
        {
            int read_old = reader.read_i;

            if (reader.TryReadString_matches_out(out string match, false, reader.lint_theme.operators, codes.Keys))
            {
                Codes code = codes[match];
                if (TryUnary(reader, tscope, expected_type, out ast_unary))
                {
                    ast_unary = new AstUnary(code, ast_unary);
                    return true;
                }
                else
                {
                    reader.Error($"expected expression after unary operator '{match}'.");
                    goto failure;
                }
            }
            // postfix
            else if (AstPrimary.TryPrimary(reader, tscope, expected_type, out ast_unary))
            {
                if (reader.TryReadChar_match('['))
                {
                    reader.LintOpeningBraquet();
                    if (TryExpr(reader, tscope, false, typeof(int), out var ast_index))
                        if (reader.TryReadChar_match(']'))
                        {
                            reader.LintClosingBraquet();
                            ast_unary = new AstIndexer(ast_unary, ast_index, ast_unary.output_type);
                        }
                        else
                        {
                            reader.Error($"expected ']' after indexer");
                            goto failure;
                        }
                    else
                    {
                        reader.Error($"expected expression after '['");
                        goto failure;
                    }
                }

                if (AstAccessor.TryAccessor(reader, ast_unary, out var ast_accessor))
                    ast_unary = ast_accessor;
                else if (reader.sig_error != null)
                    goto failure;

                return true;
            }
            else
            {
                reader.Error($"could not parse factor");
                read_old = reader.read_i;
                goto failure;
            }

        failure:
            reader.read_i = read_old;
            ast_unary = null;
            return false;
        }

        //----------------------------------------------------------------------------------------------------------

        internal override void OnExecutionStack(in Janitor janitor)
        {
            base.OnExecutionStack(janitor);

            //var factor = exec_stack._stack[^1];

            //Executor executor = new("unary", expected_type);
            //exec_stack._stack.Add(executor);

            //if (signal.arm_executors)
            //    executor.action_SIG_EXE = exe =>
            //    {
            //        exe.output = factor.output switch
            //        {
            //            bool b => code switch
            //            {
            //                OP_CODES.NOT => !b,
            //                _ => throw new Exception(),
            //            },
            //            int i => code switch
            //            {
            //                OP_CODES.ADD => i,
            //                OP_CODES.SUBSTRACT => -i,
            //                _ => throw new Exception(),
            //            },
            //            float f => code switch
            //            {
            //                OP_CODES.ADD => f,
            //                OP_CODES.SUBSTRACT => -f,
            //                _ => throw new Exception(),
            //            },
            //            _ => throw new NotImplementedException(),
            //        };
            //    };
        }
    }
}