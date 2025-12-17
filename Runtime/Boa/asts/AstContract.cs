using System;
using System.Collections.Generic;
using System.Linq;

namespace _COBRA_.Boa
{
    internal class AstContract : AstExpression
    {
        class TOptions : List<(DevContract.OptionKey name, AstExpression ast)>
        {
        }

        class TArgs : List<AstExpression>
        {
        }

        readonly DevContract contract;
        readonly TOptions topts;
        readonly TArgs targs;

        //----------------------------------------------------------------------------------------------------------

        AstContract(in DevContract contract, in TOptions topts, in TArgs targs) : base(contract.output_type)
        {
            this.contract = contract;
            this.topts = topts;
            this.targs = targs;
        }

        //----------------------------------------------------------------------------------------------------------

        public static bool TryParseContract(in CodeReader reader, in MemScope tscope, in Type expected_type, out AstContract ast_contract)
        {
            if (reader.TryReadString_matches_out(out string cont_name, as_function_argument: false, lint: reader.lint_theme.contracts, matches: DevContract.contracts.Keys.ToArray()))
                if (!DevContract.contracts.TryGetValue(cont_name, out var contract))
                {
                    reader.CompilationError($"no contract named '{cont_name}'.");
                    goto failure;
                }
                else if (expected_type != null && (contract.output_type == null || !expected_type.IsAssignableFrom(contract.output_type)))
                {
                    reader.CompilationError($"expected contract of type {expected_type}, got {contract.output_type}");
                    goto failure;
                }
                else
                {
                    TOptions topts = null;
                    TArgs targs = null;

                    if (contract.options != null)
                    {
                        topts = new();
                        foreach (var pair in contract.options)
                            if (pair.Value != null)
                                if (TryExpr(reader, tscope, false, pair.Value, out var ast_expr))
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
                        reader.CompilationError($"'{contract.name}' expected opening parenthesis '('");
                        goto failure;
                    }

                    if (contract.arguments != null)
                    {
                        targs = new();
                        for (int i = 0; i < contract.arguments.Count; i++)
                        {
                            Type arg_type = contract.arguments[i];
                            if (TryExpr(reader, tscope, true, arg_type, out var ast_expr))
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
                        reader.CompilationError($"'{contract.name}' expected closing parenthesis ')'");
                        goto failure;
                    }

                    ast_contract = new AstContract(contract, topts, targs);
                    return true;
                }

            failure:
            ast_contract = null;
            return false;
        }

        //----------------------------------------------------------------------------------------------------------

        protected override void OnExecutionQueue(in Janitor janitor, in List<Executor> executors)
        {
            base.OnExecutionQueue(janitor, executors);

            DevContract.VOptions vopts = null;
            DevContract.VArguments vargs = null;

            if (topts != null)
            {
                vopts = new();
                for (int i = 0; i < topts.Count; i++)
                    topts[i].ast.EnqueueExecutors(janitor);
            }

            if (targs != null)
            {
                vargs = new();
                for (int i = 0; i < targs.Count; i++)
                    targs[i].EnqueueExecutors(janitor);
            }

            janitor.executors.Enqueue(new(
                name: $"pop options and arguments for contract({contract.name})",
                action_SIG_EXE: janitor =>
                {
                    if (targs != null)
                        for (int i = targs.Count - 1; i >= 0; i--)
                            vargs.Add(janitor.vstack.PopLast());

                    if (topts != null)
                        for (int i = topts.Count - 1; i >= 0; i--)
                            vopts.Add(topts[i].name, janitor.vstack.PopLast());
                }
            ));

            DevContract.Parameters prms = new(janitor, vopts, vargs);

            if (contract.action != null)
                janitor.executors.Enqueue(new(
                    name: $"action(tick) for contract({contract.name})",
                    action_SIG_EXE: janitor => contract.action(janitor, prms)
                ));

            if (contract.routine != null)
                janitor.executors.Enqueue(new(
                    name: $"routine(tick) for contract({contract.name})",
                    routine_SIG_EXE: janitor => contract.routine(janitor, prms)
                ));

            if (contract.routine_READER != null)
                janitor.executors.Enqueue(new(
                    name: $"routine(reader) for contract({contract.name})",
                    routine_SIG_READER: janitor => contract.routine_READER(janitor, prms)
                ));
        }
    }
}