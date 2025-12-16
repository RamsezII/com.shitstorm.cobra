namespace _COBRA_.Boa.compilation
{
    internal abstract class AstStatement : AstAbstract
    {
        public static bool TryStatement(in CodeReader reader, in TScope tscope, out AstStatement ast_statement)
        {
        skipped_comments:
            if (reader.TryReadChar_match('#', lint: reader.lint_theme.comments))
            {
                reader.SkipUntil('\n');
                goto skipped_comments;
            }

            if (reader.TryReadChar_match(';', lint: reader.lint_theme.command_separators))
            {
                ast_statement = null;
                return true;
            }

            if (AstBlock.TryBlock(reader, tscope, out var ast_block))
            {
                ast_statement = ast_block;
                return true;
            }
            else if (AstExprStatement.TryExprStatement(reader, tscope, out var ast_expr))
            {
                ast_statement = ast_expr;
                return true;
            }

            ast_statement = null;
            return false;
        }

        //----------------------------------------------------------------------------------------------------------

        internal override void OnExecutionStack(in execution.Janitor janitor)
        {
            base.OnExecutionStack(janitor);
        }
    }

    internal sealed class AstExprStatement : AstStatement
    {
        readonly AstExpression expression;

        //----------------------------------------------------------------------------------------------------------

        AstExprStatement(in AstExpression expression)
        {
            this.expression = expression;
        }

        //----------------------------------------------------------------------------------------------------------

        public static bool TryExprStatement(in CodeReader reader, in TScope tscope, out AstExprStatement ast_statement)
        {
            if (AstExpression.TryExpr(reader, tscope, false, null, out var expression))
            {
                ast_statement = new AstExprStatement(expression);
                return true;
            }

            if (!reader.TryReadChar_match(';', lint: reader.lint_theme.command_separators))
                if (reader.strict_syntax)
                {
                    reader.Error($"Expected ';' at the end of statement");
                    goto failure;
                }

            failure:
            ast_statement = null;
            return false;
        }

        //----------------------------------------------------------------------------------------------------------

        internal override void OnExecutionStack(in execution.Janitor janitor)
        {
            base.OnExecutionStack(janitor);
        }
    }
}