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

        protected internal override void OnExecutorsQueue(in Queue<Executor> executors)
        {
            base.OnExecutorsQueue(executors);

            bool cond = false;

            List<Executor> exe_yes = new(), exe_no = new();

            ast_cond.OnExecutorsQueue(executors);

            executors.Enqueue(new(
                name: $"ternary(retreive cond: {ast_cond} ? {ast_yes} : {ast_no})",
                action_SIG_EXE: janitor =>
                {
                    var cell = janitor.vstack.PopLast();
                    cond = (bool)cell._value;
                }
            ));

            executors.Enqueue(new(
                name: $"ternary({cond}:{(cond ? ast_yes : ast_no)})",
                action_SIG_EXE: janitor =>
                {
                    foreach (var exe in cond ? exe_no : exe_yes)
                        exe.Dispose();
                }
            ));

            int index1 = executors.Count;
            ast_yes.OnExecutorsQueue(executors);
            int index2 = executors.Count;
            ast_no.OnExecutorsQueue(executors);
            int index3 = executors.Count;

            int index = 0;
            foreach (var exe in executors)
            {
                if (index >= index1 && index < index2)
                    exe_yes.Add(exe);
                else if (index >= index2 && index < index3)
                    exe_no.Add(exe);
                ++index;
            }
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