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

        public static bool TryBlock(in CodeReader reader, in MemScope scope, out AstBlock ast_block)
        {
            if (reader.TryReadChar_match('{'))
            {
                reader.LintOpeningBraquet();

                var sub_scope = new MemScope(scope);
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

        internal override void OnExecutionStack(Janitor janitor)
        {
            base.OnExecutionStack(janitor);

            for (int i = asts.Count - 1; i >= 0; i--)
                asts[i].OnExecutionStack(null);
        }
    }
}