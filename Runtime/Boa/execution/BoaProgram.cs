using System.Collections.Generic;

namespace _COBRA_.Boa
{
    internal sealed class BoaProgram
    {
        public readonly TScope tscope = new(parent: null);

        public readonly Queue<AstAbstract> asts = new();

        public readonly bool execute_in_background;

        //----------------------------------------------------------------------------------------------------------

        public BoaProgram(in CodeReader reader)
        {
            tscope._vars.Add("_args_", typeof(string[]));
            tscope._vars.Add("_home_dir_", typeof(string));

#if UNITY_EDITOR
            tscope._vars.Add("_assets_dir_", typeof(string));
#endif

            while (reader.HasNext() && AstStatement.TryStatement(reader, tscope, out var ast))
                if (ast != null)
                    asts.Enqueue(ast);

            if (reader.sig_error != null)
                return;

            execute_in_background = reader.TryReadChar_match('&', lint: reader.lint_theme.command_separators);

            if (reader.TryPeekChar_out(out char peek, out _))
                reader.CompilationError($"could not parse everything ({nameof(peek)}: '{peek}').");
        }
    }
}