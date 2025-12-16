using System;

namespace _COBRA_.Boa.compilation
{
    internal class AstVariable : AstExpression
    {
        readonly string var_name;

        //----------------------------------------------------------------------------------------------------------

        public AstVariable(in string var_name, in Type output_type) : base(output_type)
        {
        }

        //----------------------------------------------------------------------------------------------------------

        public static bool TryParseVariable(in CodeReader reader, in TScope tscope, in Type expected_type, out AstVariable ast_variable)
        {
            ast_variable = null;
            return false;
        }
    }
}