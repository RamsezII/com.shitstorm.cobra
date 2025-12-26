using System.Collections.Generic;

namespace _COBRA_.Boa
{
    internal sealed class AstBlock : AstStatement
    {
        readonly List<AstStatement> asts;

        //----------------------------------------------------------------------------------------------------------

        AstBlock(in List<AstStatement> asts)
        {
            this.asts = asts;
        }

        //----------------------------------------------------------------------------------------------------------

        public static bool TryBlock(in CodeReader reader, in MemScope memscope, out AstBlock ast_block)
        {
            if (reader.TryReadChar_match('{'))
            {
                reader.LintOpeningBraquet();

                var sub_scope = memscope.GetSubScope("block_scope");
                var asts = new List<AstStatement>();

                while (TryStatement(reader, sub_scope, out var ast_statement))
                    asts.Add(ast_statement);

                if (reader.sig_error != null)
                    goto failure;

                if (reader.TryReadChar_match('}', lint: reader.CloseBraquetLint()))
                {
                    ast_block = new AstBlock(asts);
                    return true;
                }
                else
                {
                    reader.CompilationError($"expected closing bracket '}}'.");
                    goto failure;
                }
            }

        failure:
            ast_block = null;
            return false;
        }

        //----------------------------------------------------------------------------------------------------------

        protected internal override void OnExecutorsQueue(MemStack memstack, MemScope memscope, in Queue<Executor> executors)
        {
            base.OnExecutorsQueue(memstack, memscope, executors);

            var subscope = memscope.GetSubScope("block_scope");

            for (int i = 0; i < asts.Count; ++i)
                asts[i].OnExecutorsQueue(memstack, subscope, executors);
        }
    }
}