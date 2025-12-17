using System;
using System.Collections;

namespace _COBRA_.Boa
{
    internal class AstIndexer : AstExpression
    {
        readonly AstExpression ast_expr, ast_idx;

        //----------------------------------------------------------------------------------------------------------

        public AstIndexer(in AstExpression ast_expr, in AstExpression ast_idx, in Type output_type) : base(output_type)
        {
            this.ast_expr = ast_expr;
            this.ast_idx = ast_idx;
        }

        //----------------------------------------------------------------------------------------------------------

        protected internal override void OnExecutorsQueue(in Janitor janitor)
        {
            base.OnExecutorsQueue(janitor);

            MemCell cell_index = default;

            ast_expr.OnExecutorsQueue(janitor);

            janitor.executors.Enqueue(new(
                name: $"indexer(retreive indexable)",
                action_SIG_EXE: janitor =>
                {
                    cell_index = janitor.vstack.PopLast();
                }
            ));

            ast_idx.OnExecutorsQueue(janitor);

            janitor.executors.Enqueue(new(
                name: $"indexer(retreive and apply index)",
                action_SIG_EXE: janitor =>
                {
                    MemCell popped = janitor.vstack.PopLast();
                    object value = ((IList)popped._value)[cell_index];
                    janitor.vstack.Add(new MemCell(value));
                }
            ));
        }
    }
}