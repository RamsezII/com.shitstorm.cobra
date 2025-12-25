using System;
using System.Collections.Generic;

namespace _COBRA_.Boa
{
    internal class AstField : AstExpression
    {
        readonly AstExpression ast_expr;
        readonly DevField field;

        //----------------------------------------------------------------------------------------------------------

        AstField(in AstExpression ast_expr, in DevField field) : base(field.output_type)
        {
            this.ast_expr = ast_expr;
            this.field = field;
        }

        //----------------------------------------------------------------------------------------------------------

        protected internal override void OnExecutorsQueue(MemStack memstack, MemScope memscope, in Queue<Executor> executors)
        {
            base.OnExecutorsQueue(memstack, memscope, executors);

            ast_expr.OnExecutorsQueue(memstack, memscope, executors);

            executors.Enqueue(new(
                name: $"field({field})",
                scope: memscope,
                action_SIG_EXE: () =>
                {
                    MemCell popped = memstack.PopLast();
                    field.OnExecution(memstack, memscope, popped._value);
                }
            ));
        }

        //----------------------------------------------------------------------------------------------------------

        public static bool TryField(in CodeReader reader, in AstExpression ast_expr, out AstField result)
        {
            Type target_type = ast_expr.output_type;
            int read_old = reader.read_i;

            Dictionary<string, DevField> cands = new();

            foreach (var fields in DevField.all_fields)
                if (fields.Key.IsAssignableFrom(target_type))
                    foreach (var field in fields.Value)
                        cands.Add(field.Key, field.Value);

            if (cands.Count == 0)
                goto failure;
            else if (reader.TryReadString_matches_out(out string match, false, reader.lint_theme.attributes, cands.Keys))
            {
                var field = cands[match];
                result = new AstField(ast_expr, field);
                return true;
            }

        failure:
            reader.read_i = read_old;
            result = null;
            return false;
        }
    }
}