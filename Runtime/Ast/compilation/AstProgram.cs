using System.Collections.Generic;

namespace _COBRA_.Boa.compilation
{
    internal sealed class AstProgram
    {
        public readonly TScope tscope = new(parent: null);

        public readonly List<AstStatement> asts = new();

        public readonly bool execute_in_background;

        //----------------------------------------------------------------------------------------------------------

        public AstProgram(in CodeReader reader)
        {
            tscope._vars.Add("_args_", typeof(string[]));
            tscope._vars.Add("_home_dir_", typeof(string));

#if UNITY_EDITOR
            tscope._vars.Add("_assets_dir_", typeof(string));
#endif

            while (reader.HasNext() && AstStatement.TryStatement(reader, tscope, out var ast))
                asts.Add(ast);

            if (reader.sig_error != null)
                return;

            execute_in_background = reader.TryReadChar_match('&', lint: reader.lint_theme.command_separators);

            if (reader.TryPeekChar_out(out char peek, out _))
                reader.Error($"could not parse everything ({nameof(peek)}: '{peek}').");
        }
    }
}