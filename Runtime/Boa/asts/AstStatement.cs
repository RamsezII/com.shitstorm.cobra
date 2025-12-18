using System.Collections.Generic;

namespace _COBRA_.Boa
{
    internal abstract class AstStatement : AstAbstract
    {
        public static bool TryStatement(in CodeReader reader, in MemScope tscope, out AstStatement ast_statement)
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
            else if (AstAssignation.TryDeclaration(reader, tscope, out var ast_decl))
            {
                ast_statement = ast_decl;
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

        public static bool TryExprStatement(in CodeReader reader, in MemScope tscope, out AstExprStatement ast_statement)
        {
            if (AstExpression.TryExpr(reader, tscope, false, null, out var expression))
            {
                ast_statement = new AstExprStatement(expression);
                if (!reader.TryReadChar_match(';', lint: reader.lint_theme.command_separators))
                    if (reader.strict_syntax)
                    {
                        reader.CompilationError($"Expected ';' at the end of statement");
                        goto failure;
                    }
                return true;
            }

        failure:
            ast_statement = null;
            return false;
        }

        //----------------------------------------------------------------------------------------------------------

        protected internal override void OnExecutorsQueue(in Janitor janitor)
        {
            base.OnExecutorsQueue(janitor);

            expression.OnExecutorsQueue(janitor);

            if (expression.output_type != null)
                janitor.executors.Enqueue(new(
                    name: "expr_statement(pop vstack last)",
                    action_SIG_EXE: static janitor =>
                    {
                        MemCell cell = janitor.vstack.PopLast();
                        if (cell._value is not BoaNull)
                            janitor.shell.on_output(cell._value, null);
                    }));
        }
    }
}