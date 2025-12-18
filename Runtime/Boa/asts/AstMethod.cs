using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using static UnityEngine.GraphicsBuffer;

namespace _COBRA_.Boa
{
    internal class AstMethod : AstExpression
    {
        readonly AstExpression ast_target;
        readonly List<AstExpression> ast_args;
        readonly DevMethod method;

        //----------------------------------------------------------------------------------------------------------

        AstMethod(in AstExpression ast_target, in List<AstExpression> ast_args, in DevMethod method) : base(method.output_type)
        {
            this.ast_target = ast_target;
            this.ast_args = ast_args;
            this.method = method;
        }

        //----------------------------------------------------------------------------------------------------------

        protected internal override void OnExecutorsQueue(in Janitor janitor)
        {
            base.OnExecutorsQueue(janitor);

            ast_target.OnExecutorsQueue(janitor);

            if (ast_args != null)
                for (int i = 0; i < ast_args.Count; i++)
                    ast_args[i].OnExecutorsQueue(janitor);

            janitor.executors.Enqueue(new(
                name: $"field({method})",
                action_SIG_EXE: janitor =>
                {
                    List<MemCell> cells = new(ast_args.Count);

                    if (ast_args != null)
                    {
                        for (int i = ast_args.Count; i > 0; --i)
                            cells.Add(janitor.vstack[^i]);
                        janitor.vstack.RemoveRange(janitor.vstack.Count - ast_args.Count, ast_args.Count);
                    }

                    MemCell popped = janitor.vstack.PopLast();
                    method.OnExecution(janitor, cells, popped._value);
                }
            ));
        }

        //----------------------------------------------------------------------------------------------------------

        public static bool TryMethod(in CodeReader reader, in MemScope scope, in AstExpression ast_target, out AstMethod ast_method)
        {
            Type target_type = ast_target.output_type;
            int read_old = reader.read_i;

            if (reader.TryReadPrefixeString_match(reader.lint_theme.point, "->"))
            {
                Dictionary<string, DevMethod> cands = new();

                foreach (var methods in DevMethod.all_methods)
                    if (methods.Key.IsAssignableFrom(target_type))
                        foreach (var method in methods.Value)
                            cands.Add(method.Key, method.Value);

                if (cands.Count == 0)
                    goto failure;
                else if (reader.TryReadString_matches_out(out string match, false, reader.lint_theme.attributes, cands.Keys))
                {
                    var method = cands[match];

                    bool expects_parenthesis = reader.strict_syntax;
                    bool found_parenthesis = reader.TryReadChar_match('(');

                    if (found_parenthesis)
                        reader.LintOpeningBraquet();

                    if (expects_parenthesis && !found_parenthesis)
                    {
                        reader.CompilationError($"'{method.name}' expected opening parenthesis '('");
                        goto failure;
                    }

                    List<AstExpression> ast_args = null;
                    if (method.targs != null)
                    {
                        ast_args = new();
                        for (int i = 0; i < method.targs.Count; i++)
                        {
                            Type arg_type = method.targs[i];
                            if (TryExpr(reader, scope, true, arg_type, out var ast_expr))
                                ast_args.Add(ast_expr);
                            else
                            {
                                reader.CompilationError($"could not parse argument[{i}] ({arg_type})");
                                goto failure;
                            }
                        }
                    }

                    if (reader.sig_error != null)
                        goto failure;

                    if ((expects_parenthesis || found_parenthesis) && !reader.TryReadChar_match(')', lint: reader.CloseBraquetLint()))
                    {
                        reader.CompilationError($"'{method.name}' expected closing parenthesis ')'");
                        goto failure;
                    }

                    ast_method = new AstMethod(ast_target, ast_args, method);
                    return true;
                }
            }

        failure:
            reader.read_i = read_old;
            ast_method = null;
            return false;
        }
    }
}