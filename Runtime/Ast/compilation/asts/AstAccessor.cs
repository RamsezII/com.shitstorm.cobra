using System;

namespace _COBRA_.Boa.compilation
{
    internal class AstAccessor : AstExpression
    {
        readonly AstExpression ast_expr;
        readonly DevAttribute attribute;

        //----------------------------------------------------------------------------------------------------------

        AstAccessor(in AstExpression ast_expr, in DevAttribute attribute) : base(attribute.output_type)
        {
            this.ast_expr = ast_expr;
            this.attribute = attribute;
        }

        //----------------------------------------------------------------------------------------------------------

        public static bool TryAccessor(in CodeReader reader, in AstExpression ast_expr, out AstAccessor result)
        {
            Type type = ast_expr.output_type;
            if (DevAttribute._attributes.TryGetValue(type, out var attributes))
                if (reader.TryReadString_match("->", false, reader.lint_theme.point))
                    if (reader.TryReadString_matches_out(out string match, false, reader.lint_theme.attributes, attributes.Keys))
                    {
                        var attribute = attributes[match];
                        result = new AstAccessor(ast_expr, attribute);
                        return true;
                    }
                    else
                    {
                        reader.Error($"{type} has no attribute named \"{match}\"");
                        goto failure;
                    }
                else
                {
                    reader.Error($"{type} has no attributes");
                    goto failure;
                }

            failure:
            result = null;
            return false;
        }
    }
}