using System;

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

            MemCell cell_index = null;

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
                    MemCell indexable = janitor.vstack.PopLast();
                    object value = indexable.AsBoa.AsList[cell_index.AsBoa];
                    janitor.vstack.Add(new MemCell(value));
                }
            ));
        }
    }
}