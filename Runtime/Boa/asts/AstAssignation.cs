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

        protected internal override void OnExecutorsQueue(MemStack memstack, MemScope memscope, in Queue<Executor> executors)
        {
            base.OnExecutorsQueue(memstack, memscope, executors);

            ast_expr.OnExecutorsQueue(memstack, memscope, executors);

            executors.Enqueue(new(
                name: $"var({var_name})",
                scope: memscope,
                action_SIG_EXE: () =>
                {
                    MemCell popped = memstack.PopLast();
                    if (!memscope.TryGetVariable(var_name, out var existant))
                        memscope._vars.Add(var_name, popped);
                    else
                    {
                        MemCell assigned = code switch
                        {
                            Codes.Incr => existant + popped,
                            Codes.Decr => existant - popped,
                            Codes.Mult => existant * popped,
                            Codes.Divide => existant / popped,
                            Codes.Modulo => existant % popped,
                            Codes.And => existant && popped,
                            Codes.Or => existant || popped,
                            Codes.Xor => existant ^= popped,
                            Codes.No => !popped,
                            _ => popped,
                        };
                        memscope.TrySetVariable(var_name, assigned);
                    }
                }
            ));
        }

        //----------------------------------------------------------------------------------------------------------

        public static bool TryAssign(in CodeReader reader, in MemScope scope, out AstAssignation ast_assign)
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
                    scope.TryGetVariable(var_name, out var cell);

                    if (AstExpression.TryExpr(reader, scope, false, cell._type, out AstExpression ast_expr))
                    {
                        if (!scope.TrySetVariable(var_name, new MemCell(ast_expr.output_type, null)))
                        {
                            reader.CompilationError($"could not set variable \"{var_name}\"");
                            goto failure;
                        }
                        ast_assign = new AstAssignation(var_name, ast_expr, code);
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
                        if (!scope.TrySetVariable(var_name, new MemCell(ast_expr.output_type, null)))
                        {
                            reader.CompilationError($"could not declare variable \"{var_name}\"");
                            goto failure;
                        }
                        ast_assign = new AstAssignation(var_name, ast_expr, Codes.Assign);
                        return true;
                    }

                failure:
            reader.read_i = read_old;
            ast_assign = null;
            return false;
        }
    }
}