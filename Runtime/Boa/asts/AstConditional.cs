using System;
using System.Collections.Generic;

namespace _COBRA_.Boa
{
    internal class AstConditional : AstExpression
    {
        readonly AstExpression ast_cond, ast_yes, ast_no;

        //----------------------------------------------------------------------------------------------------------

        AstConditional(in AstExpression ast_cond, in AstExpression ast_yes, in AstExpression ast_no, in Type output_type) : base(output_type)
        {
            this.ast_cond = ast_cond;
            this.ast_yes = ast_yes;
            this.ast_no = ast_no;
        }

        //----------------------------------------------------------------------------------------------------------

        protected override void OnExecutionQueue(in Janitor janitor, in List<Executor> executors)
        {
            base.OnExecutionQueue(janitor, executors);

            ast_cond.EnqueueExecutors(janitor, out var cond_list);
            ast_yes.EnqueueExecutors(janitor, out var yes_list);
            ast_no.EnqueueExecutors(janitor, out var no_list);

            janitor.executors.Enqueue(new(
                name: $"ternary(retreive cond)",
                action_SIG_EXE: janitor =>
                {
                    var cell = janitor.vstack.PopLast();
                    bool cond = cell.value;

                    foreach (var exe in cond ? no_list : yes_list)
                        exe.Dispose();
                }
            ));
        }

        //----------------------------------------------------------------------------------------------------------

        public static bool TryConditional(in CodeReader reader, in MemScope tscope, in Type expected_type, out AstExpression ast_expr)
        {
            if (AstBinaryOperation.TryOr(reader, tscope, expected_type, out ast_expr))
                if (!reader.TryReadChar_match('?', lint: reader.lint_theme.operators))
                    return true;
                else
                {
                    if (ast_expr.output_type != typeof(bool))
                    {
                        reader.CompilationError($"expected {typeof(bool)} after ternary operator '?', got {ast_expr.output_type}");
                        goto failure;
                    }

                    if (!TryExpr(reader, tscope, false, expected_type ?? typeof(object), out var expr_yes))
                        reader.CompilationError($"expected expression after ternary operator '?'");
                    else
                    {
                        if (!reader.TryReadChar_match(':', lint: reader.lint_theme.operators))
                            reader.CompilationError($"expected ternary operator delimiter ':'");
                        else
                        {
                            if (!TryConditional(reader, tscope, expected_type ?? typeof(object), out var expr_no))
                                reader.CompilationError($"expected second expression after ternary operator ':'");
                            else
                            {
                                Type output_type = Util_cobra.EnglobingType(expr_yes.output_type, expr_no.output_type);
                                if (output_type == null)
                                {
                                    reader.CompilationError($"both expression after '?' operator must return something");
                                    goto failure;
                                }

                                ast_expr = new AstConditional(ast_expr, expr_yes, expr_no, output_type);
                                return true;
                            }
                        }
                    }
                }

            failure:
            ast_expr = null;
            return false;
        }
    }
}