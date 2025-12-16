using System;

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

        public static bool TryConditional(in CodeReader reader, in TScope tscope, in Type expected_type, out AstExpression ast_expr)
        {
            if (AstBinaryOperation.TryOr(reader, tscope, expected_type, out ast_expr))
                if (!reader.TryReadChar_match('?', lint: reader.lint_theme.operators))
                    return true;
                else
                {
                    if (ast_expr.output_type != typeof(bool))
                    {
                        reader.Error($"expected {typeof(bool)} after ternary operator '?', got {ast_expr.output_type}");
                        goto failure;
                    }

                    if (!TryExpr(reader, tscope, false, expected_type ?? typeof(object), out var expr_yes))
                        reader.Error($"expected expression after ternary operator '?'");
                    else
                    {
                        if (!reader.TryReadChar_match(':', lint: reader.lint_theme.operators))
                            reader.Error($"expected ternary operator delimiter ':'");
                        else
                        {
                            if (!TryConditional(reader, tscope, expected_type ?? typeof(object), out var expr_no))
                                reader.Error($"expected second expression after ternary operator ':'");
                            else
                            {
                                Type output_type = Util_cobra.EnglobingType(expr_yes.output_type, expr_no.output_type);
                                if (output_type == null)
                                {
                                    reader.Error($"both expression after '?' operator must return something");
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