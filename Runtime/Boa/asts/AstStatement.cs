using System.Collections.Generic;

namespace _COBRA_.Boa
{
    internal abstract class AstStatement : AstAbstract
    {
        public static bool TryStatement(in CodeReader reader, in MemScope scope, out AstStatement ast_statement)
        {
        skipped_comments:
            if (reader.TryReadChar_match('#', lint: reader.lint_theme.comments))
            {
                reader.SkipUntil('\n');
                goto skipped_comments;
            }

            if (reader.HasNext() && !reader.TryPeekChar_match('}', out _))
                if (reader.TryReadChar_match(';', lint: reader.lint_theme.command_separators))
                {
                    ast_statement = null;
                    return true;
                }
                else if (AstUsrMethod_create.TryParse(reader, scope, out var createMethod))
                {
                    ast_statement = createMethod;
                    return true;
                }
                else if (AstBlock.TryBlock(reader, scope, out var ast_block))
                {
                    ast_statement = ast_block;
                    return true;
                }
                else if (AstAssignation.TryAssign(reader, scope, out var ast_decl))
                {
                    ast_statement = ast_decl;
                    return true;
                }
                else if (AstExprStatement.TryExprStatement(reader, scope, out var ast_expr))
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

        protected internal override void OnExecutorsQueue(MemStack memstack, MemScope memscope, in Queue<Executor> executors)
        {
            base.OnExecutorsQueue(memstack, memscope, executors);

            expression.OnExecutorsQueue(memstack, memscope, executors);

            if (expression.output_type != null)
                executors.Enqueue(new(
                    name: "expr_statement(pop vstack last)",
                    scope: memscope,
                    action_SIG_EXE: () =>
                    {
                        MemCell cell = memstack.PopLast();
                        if (cell._value is not BoaNull)
                            memscope.shell.stdout(cell._value, null);
                    }));
        }
    }
}