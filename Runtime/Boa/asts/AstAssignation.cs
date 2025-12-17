using System;
using System.Collections.Generic;
using System.Linq;

namespace _COBRA_.Boa
{
    internal class AstAssignation : AstStatement
    {
        enum Codes : byte
        {
            Assign,
            Incr,
            Decr,
            Mult,
            Divide,
            Modulo,
            And,
            Or,
            Xor,
            No,
        }

        static readonly Dictionary<string, Codes> codes = new(StringComparer.OrdinalIgnoreCase)
        {
            { "=", Codes.Assign },
            { "!=", Codes.No },
            { "+=", Codes.Incr },
            { "-=", Codes.Decr },
            { "*=", Codes.Mult },
            { "/=", Codes.Divide },
            { "%=", Codes.Modulo },
            { "&=", Codes.And },
            { "|=", Codes.Or },
            { "^=", Codes.Xor },
        };

        readonly string var_name;
        readonly AstExpression ast_expr;
        readonly Codes code;

        //----------------------------------------------------------------------------------------------------------

        AstAssignation(in string name, in AstExpression expr, in Codes code)
        {
            var_name = name;
            ast_expr = expr;
            this.code = code;
        }

        //----------------------------------------------------------------------------------------------------------

        protected override void OnExecutionQueue(in Janitor janitor, in List<Executor> executors)
        {
            base.OnExecutionQueue(janitor, executors);

            ast_expr.EnqueueExecutors(janitor);

            janitor.executors.Enqueue(new(
                name: $"var({var_name})",
                action_SIG_EXE: janitor =>
                {
                    MemCell stack_cell = janitor.vstack.PopLast();
                    if (!janitor.shell.scope.TryGet(var_name, out var mem_cell))
                        janitor.shell.scope._vars.Add(var_name, stack_cell);
                    else
                        mem_cell.value = code switch
                        {
                            Codes.Incr => mem_cell.value + stack_cell.value,
                            Codes.Decr => mem_cell.value - stack_cell.value,
                            Codes.Mult => mem_cell.value * stack_cell.value,
                            Codes.Divide => mem_cell.value / stack_cell.value,
                            Codes.Modulo => mem_cell.value % stack_cell.value,
                            Codes.And => mem_cell.value && stack_cell.value,
                            Codes.Or => mem_cell.value || stack_cell.value,
                            Codes.Xor => mem_cell.value ^= stack_cell.value,
                            Codes.No => !stack_cell.value,
                            _ => stack_cell.value,
                        };
                }
            ));
        }

        //----------------------------------------------------------------------------------------------------------

        public static bool TryDeclaration(in CodeReader reader, in MemScope scope, out AstAssignation ast_assign)
        {
            int read_old = reader.read_i;

            if (reader.TryReadString_matches_out(
                out string var_name,
                as_function_argument: false,
                lint: reader.lint_theme.variables,
                matches: scope.EVarNames().ToArray())
            )
            {
                if (reader.TryReadPrefixeString_matches_out(
                    out string op_name,
                    lint: reader.lint_theme.operators,
                    ignore_case: true,
                    add_to_completions: false,
                    matches: codes.Keys)
                )
                {
                    Codes code = codes[op_name];
                    scope.TryGet(var_name, out var cell);

                    if (AstExpression.TryExpr(reader, scope, false, cell.type, out AstExpression expr))
                    {
                        ast_assign = new AstAssignation(var_name, expr, code);
                        return true;
                    }
                    else
                        reader.CompilationError($"expected expression after '{op_name}' operator.");
                }
            }
            else if (reader.TryReadArgument(out var_name, false, lint: reader.lint_theme.variables))
                if (reader.TryReadChar_match('=', reader.lint_theme.operators))
                    if (AstExpression.TryExpr(reader, scope, false, typeof(object), out var ast_expr))
                    {
                        scope._vars.Add(var_name, new(type: ast_expr.output_type));
                        ast_assign = new AstAssignation(var_name, ast_expr, Codes.Assign);
                        return true;
                    }

            reader.read_i = read_old;
            ast_assign = null;
            return false;
        }
    }
}