using System;

namespace _COBRA_.Boa
{
    internal abstract class AstExpression : AstAbstract
    {
        public readonly Type output_type;

        //----------------------------------------------------------------------------------------------------------

        public AstExpression(in Type output_type)
        {
            this.output_type = output_type;
        }

        //----------------------------------------------------------------------------------------------------------

        public static bool TryExpr(in CodeReader reader, in TScope tscope, in bool read_as_argument, in Type expected_type, out AstExpression ast_expression)
        {
            if (AstAssignment.TryParseAssignment(reader, tscope, expected_type, out ast_expression))
                return true;
            else if (reader.sig_error != null)
                goto failure;

            if (AstConditional.TryConditional(reader, tscope, expected_type, out ast_expression))
            {
                if (read_as_argument)
                    if (!reader.TryReadChar_match(',', lint: reader.lint_theme.argument_coma) && !reader.TryPeekChar_match(')', out _))
                        if (reader.strict_syntax)
                        {
                            reader.Error($"expected ',' or ')' after expression");
                            goto failure;
                        }
                return true;
            }

        failure:
            ast_expression = null;
            return false;
        }
    }
}