using System;

namespace _COBRA_.Boa
{
    internal class AstAccessor : AstExpression
    {
        readonly AstExpression ast_expr;
        readonly DevField field;

        //----------------------------------------------------------------------------------------------------------

        AstAccessor(in AstExpression ast_expr, in DevField field) : base(field.type)
        {
            this.ast_expr = ast_expr;
            this.field = field;
        }

        //----------------------------------------------------------------------------------------------------------

        protected internal override void OnExecutorsQueue(in Janitor janitor)
        {
            base.OnExecutorsQueue(janitor);

            ast_expr.OnExecutorsQueue(janitor);

            janitor.executors.Enqueue(new(
                name: $"field({field})",
                action_SIG_EXE: janitor =>
                {
                    MemCell popped = janitor.vstack.PopLast();
                    field.OnTarget(janitor, popped._value);
                }
            ));
        }

        //----------------------------------------------------------------------------------------------------------

        public static bool TryAccessor(in CodeReader reader, in AstExpression ast_expr, out AstAccessor result)
        {
            Type type = ast_expr.output_type;
            if (reader.TryReadPrefixeString_matches_out(out _, reader.lint_theme.point, matches: new string[] { "->", }))
                if (DevField.all_fields.TryGetValue(type, out var attributes))
                    if (reader.TryReadString_matches_out(out string match, false, reader.lint_theme.attributes, attributes.Keys))
                    {
                        var attribute = attributes[match];
                        result = new AstAccessor(ast_expr, attribute);
                        return true;
                    }
                    else
                    {
                        reader.CompilationError($"{type} has no attribute named \"{match}\"");
                        goto failure;
                    }
                else
                {
                    reader.CompilationError($"{type} has no attributes");
                    goto failure;
                }

            failure:
            result = null;
            return false;
        }
    }
}