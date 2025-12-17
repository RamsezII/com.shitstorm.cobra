using System;
using System.Collections.Generic;

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

        protected override void OnExecutionQueue(in Janitor janitor, in List<Executor> executors)
        {
            base.OnExecutionQueue(janitor, executors);

            MemCell cell_index = null;

            ast_expr.EnqueueExecutors(janitor);

            janitor.executors.Enqueue(new(
                name: $"indexer(retreive indexable)",
                action_SIG_EXE: janitor =>
                {
                    cell_index = janitor.vstack.PopLast();
                }
            ));

            ast_idx.EnqueueExecutors(janitor);

            janitor.executors.Enqueue(new(
                name: $"indexer(retreive and apply index)",
                action_SIG_EXE: janitor =>
                {
                    MemCell cell_col = janitor.vstack.PopLast();
                    dynamic val = cell_col.value[cell_index.value];
                    janitor.vstack.Add(new(value: val));
                }
            ));
        }
    }
}