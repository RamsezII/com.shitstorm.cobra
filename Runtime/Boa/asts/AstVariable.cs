using System;
using System.Collections.Generic;

namespace _COBRA_.Boa
{
    internal class AstVariable : AstExpression
    {
        readonly string var_name;

        //----------------------------------------------------------------------------------------------------------

        public AstVariable(in string var_name, in Type var_type) : base(var_type)
        {
            this.var_name = var_name;
        }

        //----------------------------------------------------------------------------------------------------------

        protected internal override void OnExecutorsQueue(in Queue<Executor> executors)
        {
            base.OnExecutorsQueue(executors);

            executors.Enqueue(new(
                name: $"var({var_name})",
                action_SIG_EXE: janitor =>
                {
                    janitor.shell.scope.TryGetVariable(var_name, out var cell);
                    janitor.vstack.Add(cell);
                }
            ));
        }

        //----------------------------------------------------------------------------------------------------------

        public static bool TryParseVariable(in CodeReader reader, in MemScope tscope, in Type expected_type, out AstVariable ast_variable)
        {
            int read_old = reader.read_i;

            if (reader.TryReadString_matches_out(out string var_name, false, reader.lint_theme.variables, tscope.EVarNames()))
                if (tscope.TryGetVariable(var_name, out var cell))
                {
                    ast_variable = new AstVariable(var_name, cell._type);
                    return true;
                }

            reader.read_i = read_old;
            ast_variable = null;
            return false;
        }
    }
}