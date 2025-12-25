using System;
using System.Collections;
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

        protected internal override void OnExecutorsQueue(MemStack memstack, MemScope memscope, in Queue<Executor> executors)
        {
            base.OnExecutorsQueue(memstack, memscope, executors);

            MemCell cell_index = default;

            ast_expr.OnExecutorsQueue(memstack, memscope, executors);

            executors.Enqueue(new(
                name: $"indexer(retreive indexable)",
                scope: memscope,
                action_SIG_EXE: () =>
                {
                    cell_index = memstack.PopLast();
                }
            ));

            ast_idx.OnExecutorsQueue(memstack, memscope, executors);

            executors.Enqueue(new(
                name: $"indexer(retreive and apply index)",
                scope: memscope,
                action_SIG_EXE: () =>
                {
                    MemCell popped = memstack.PopLast();
                    object value = ((IList)popped._value)[cell_index];
                    memstack.Add(new MemCell(value));
                }
            ));
        }
    }
}