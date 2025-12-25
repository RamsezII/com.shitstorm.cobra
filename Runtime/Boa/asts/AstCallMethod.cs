using _UTIL_;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace _COBRA_.Boa
{
    internal class AstCallMethod : AstExpression
    {
        readonly MemMethod method;
        readonly List<(MemMethod.OptionKey names, AstExpression ast)> asts_opts = new();
        readonly List<AstExpression> asts_args = new();

        //----------------------------------------------------------------------------------------------------------

        AstCallMethod(in MemMethod method, in List<(MemMethod.OptionKey, AstExpression)> asts_opts, in List<AstExpression> asts_args) : base(method.output_type)
        {
            this.method = method;
            this.asts_opts = asts_opts;
            this.asts_args = asts_args;
        }

        //----------------------------------------------------------------------------------------------------------

        protected internal override void OnExecutorsQueue(in Queue<Executor> executors)
        {
            base.OnExecutorsQueue(executors);

            if (asts_opts != null)
            {
                for (int i = 0; i < asts_opts.Count; i++)
                    asts_opts[i].ast.OnExecutorsQueue(executors);

                executors.Enqueue(new(
                    name: $"pop options for method({method.name})",
                    action_SIG_EXE: janitor =>
                    {
                        Debug.LogWarning($"TODO: add options to scope");

                        for (int i = asts_opts.Count; i > 0; i--)
                        {
                            var (names, ast) = asts_opts[^i];
                            MemCell cell = janitor.vstack[^i];

                            MemScope scope = new();
                            scope._vars.Add(names.short_name.ToString(), cell);
                            scope._vars.Add(names.long_name, cell);
                        }

                        janitor.vstack.RemoveRange(janitor.vstack.Count - asts_opts.Count, asts_opts.Count);
                    }
                ));
            }

            if (asts_args != null)
            {
                for (int i = 0; i < asts_args.Count; i++)
                    asts_args[i].OnExecutorsQueue(executors);

                executors.Enqueue(new(
                    name: $"pop arguments for method( {method.name})",
                    action_SIG_EXE: janitor =>
                    {
                        Debug.LogWarning($"TODO: add arguments to scope");

                        for (int i = asts_args.Count; i > 0; i--)
                        {
                            var ast = asts_args[i];
                            MemCell cell = janitor.vstack[^i];

                            MemScope scope = new();
                            scope._vars.Add(method.targs[i].name, cell);
                        }

                        janitor.vstack.RemoveRange(janitor.vstack.Count - asts_args.Count, asts_args.Count);
                    }
                ));
            }

            method.ast.OnExecutorsQueue(executors);

            executors.Enqueue(new(
                name: "push empty in stack",
                action_SIG_EXE: static janitor => janitor.vstack.Add(new MemCell(new NamedDummy("method call")))
            ));
        }

        //----------------------------------------------------------------------------------------------------------

        public static bool TryParseCall(in CodeReader reader, in MemScope scope, in Type expected_type, out AstCallMethod ast_call)
        {
            if (reader.TryReadString_matches_out(out string met_name, false, reader.lint_theme.functions, scope.EMetNames()))
                if (!scope.TryGetMethod(met_name, out var method))
                {
                    reader.CompilationError($"no method named '{met_name}'.");
                    goto failure;
                }
                else if (expected_type != null && (method.output_type == null || !expected_type.IsAssignableFrom(method.output_type)))
                {
                    reader.CompilationError($"expected method of type {expected_type}, got {method.output_type}");
                    goto failure;
                }
                else
                {
                    List<(MemMethod.OptionKey names, AstExpression ast)> topts = null;
                    List<AstExpression> targs = null;

                    if (method.topts != null)
                    {
                        topts = new();
                        foreach (var pair in method.topts)
                            if (pair.Value != null)
                                if (TryExpr(reader, scope, false, pair.Value, out var ast_expr))
                                    topts.Add((pair.Key, ast_expr));
                                else
                                {
                                    reader.CompilationError($"could not parse expression for option {pair.Key}");
                                    goto failure;
                                }
                    }

                    bool expects_parenthesis = reader.strict_syntax;
                    bool found_parenthesis = reader.TryReadChar_match('(');

                    if (found_parenthesis)
                        reader.LintOpeningBraquet();

                    if (expects_parenthesis && !found_parenthesis)
                    {
                        reader.CompilationError($"'{method.name}' expected opening parenthesis '('");
                        goto failure;
                    }

                    if (method.targs != null)
                    {
                        targs = new();
                        for (int i = 0; i < method.targs.Count; i++)
                        {
                            Type arg_type = method.targs[i].type;
                            if (TryExpr(reader, scope, true, arg_type, out var ast_expr))
                                targs.Add(ast_expr);
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

                    ast_call = new AstCallMethod(method, topts, targs);
                    return true;
                }

            failure:
            ast_call = null;
            return false;
        }
    }
}