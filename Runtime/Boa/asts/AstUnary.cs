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

        protected internal override void OnExecutorsQueue(in Janitor janitor)
        {
            base.OnExecutorsQueue(janitor);

            ast_factor.OnExecutorsQueue(janitor);

            janitor.executors.Enqueue(new(
                name: $"unary({code})",
                action_SIG_EXE: janitor =>
                {
                    MemCell popped = janitor.vstack.PopLast();
                    MemCell assigned = code switch
                    {
                        Codes.Positive => +popped,
                        Codes.Negative => -popped,
                        Codes.Not => !popped,
                        Codes.Anti => ~popped,
                        _ => throw new NotImplementedException($"(unary) unimplemented code \"{code}\""),
                    };
                    janitor.vstack.Add(assigned);
                }
            ));
        }

        //----------------------------------------------------------------------------------------------------------

        public static bool TryUnary(in CodeReader reader, in MemScope tscope, in Type expected_type, out AstExpression ast_unary)
        {
            int read_old = reader.read_i;

            if (reader.TryReadPrefixeString_matches_out(out string match, reader.lint_theme.operators, codes.Keys))
            {
                Codes code = codes[match];
                if (TryUnary(reader, tscope, expected_type, out ast_unary))
                {
                    ast_unary = new AstUnary(code, ast_unary);
                    return true;
                }
                else
                {
                    reader.CompilationError($"expected expression after unary operator '{match}'.");
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
                            reader.CompilationError($"expected ']' after indexer");
                            goto failure;
                        }
                    else
                    {
                        reader.CompilationError($"expected expression after '['");
                        goto failure;
                    }
                }

                if (ast_unary.output_type != null)
                    if (AstAccessor.TryAccessor(reader, ast_unary, out var ast_accessor))
                        ast_unary = ast_accessor;
                    else if (reader.sig_error != null)
                        goto failure;

                return true;
            }
            else
            {
                reader.CompilationError($"could not parse factor");
                read_old = reader.read_i;
                goto failure;
            }

        failure:
            reader.read_i = read_old;
            ast_unary = null;
            return false;
        }
    }
}