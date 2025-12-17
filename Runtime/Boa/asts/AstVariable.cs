using System;

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

        protected internal override void OnExecutorsQueue(in Janitor janitor)
        {
            base.OnExecutorsQueue(janitor);

            janitor.shell.scope.TryGet(var_name, out var cell);
            janitor.vstack.Add(cell);
        }

        //----------------------------------------------------------------------------------------------------------

        public static bool TryParseVariable(in CodeReader reader, in MemScope tscope, in Type expected_type, out AstVariable ast_variable)
        {
            int read_old = reader.read_i;

            if (reader.TryReadString_matches_out(out string var_name, false, reader.lint_theme.variables, tscope.EVarNames()))
                if (tscope.TryGet(var_name, out var cell))
                {
                    ast_variable = new AstVariable(var_name, cell.type);
                    return true;
                }

            reader.read_i = read_old;
            ast_variable = null;
            return false;
        }
    }
}