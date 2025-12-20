using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace _COBRA_.Boa
{
    internal class AstString : AstExpression
    {
        readonly List<AstExpression> asts = new();

        //----------------------------------------------------------------------------------------------------------

        AstString(in List<AstExpression> asts) : base(typeof(string))
        {
            this.asts = asts;
        }

        //----------------------------------------------------------------------------------------------------------

        protected internal override void OnExecutorsQueue(in Queue<Executor> executors)
        {
            base.OnExecutorsQueue(executors);

            for (int i = 0; i < asts.Count; i++)
                asts[i].OnExecutorsQueue(executors);

            executors.Enqueue(new(
                name: $"string({asts.Count})",
                action_SIG_EXE: janitor =>
                {
                    StringBuilder sb = new();
                    for (int i = asts.Count; i > 0; i--)
                    {
                        string s = janitor.vstack[^i]._value.ToString();
                        sb.Append(s);
                    }
                    janitor.vstack.RemoveRange(janitor.vstack.Count - asts.Count, asts.Count);
                    janitor.vstack.Add(new MemCell(sb.ToString()));
                }
            ));
        }

        //----------------------------------------------------------------------------------------------------------

        public static bool TryParseString(in CodeReader reader, in MemScope tscope, out AstString ast_string)
        {
            int read_old = reader.read_i;

            char sep = default;

            if (reader.TryReadChar_match('\'', lint: reader.lint_theme.quotes))
                sep = '\'';
            else if (reader.TryReadChar_match('"', lint: reader.lint_theme.quotes))
                sep = '"';

            if (sep == default)
            {
                ast_string = null;
                return false;
            }

            reader.cpl_start = Mathf.Min(read_old + 1, reader.read_i - 1);
            reader.cpl_end = reader.read_i - 1;

            List<AstExpression> asts = new();
            string current_fragment = string.Empty;
            int start_i = reader.read_i;
            bool flag_escape = false;

            while (reader.TryReadChar_out(out char c, skippables: null))
                switch (c)
                {
                    // escape character
                    case '\\':
                        reader.LintToThisPosition(reader.lint_theme.strings, false, reader.read_i - 1);
                        flag_escape = true;
                        reader.LintToThisPosition(reader.lint_theme.quotes, false);
                        break;

                    // expression
                    case '{' when !flag_escape:
                        {
                            reader.LintToThisPosition(reader.lint_theme.strings, false, reader.read_i - 1);
                            reader.LintToThisPosition(reader.lint_theme.quotes, false);

                            if (current_fragment.Length > 0)
                                asts.Add(new AstLiteral(current_fragment));

                            if (TryExpr(reader, tscope, false, typeof(object), out AstExpression expression))
                                asts.Add(expression);
                            else
                            {
                                reader.CompilationError($"expected expression after '{{'.");
                                goto failure;
                            }

                            if (!reader.TryReadChar_match('}'))
                            {
                                reader.CompilationError($"expected closing braquet '}}'.");
                                goto failure;
                            }

                            reader.LintToThisPosition(reader.lint_theme.strings, false, reader.read_i - 1);
                            reader.LintToThisPosition(reader.lint_theme.quotes, false);
                        }
                        break;

                    // end of string
                    case '\'' or '"' when !flag_escape && c == sep:
                        {
                            reader.LintToThisPosition(reader.lint_theme.strings, false, reader.read_i - 1);
                            reader.LintToThisPosition(reader.lint_theme.quotes, false);

                            reader.last_arg = current_fragment;
                            reader.cpl_end = reader.read_i - 1;

                            if (current_fragment.Length > 0)
                                asts.Add(new AstLiteral(current_fragment));

                            ast_string = new AstString(asts);
                        }
                        return true;

                    // validate char
                    default:
                        flag_escape = false;
                        current_fragment += c;
                        break;
                }

            if (current_fragment.TryIndexOf_min(out int err_index, 0, true, ' ', '\t', '\n', '\r'))
                reader.read_i = start_i + err_index;
            else
                reader.read_i = read_old;

            reader.LintToThisPosition(reader.lint_theme.quotes, false);

        failure:
            reader.CompilationError($"string error: expected closing quote '{sep}'.");
            ast_string = null;
            return false;
        }
    }
}