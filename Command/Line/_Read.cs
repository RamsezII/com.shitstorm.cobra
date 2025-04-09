using System.Collections.Generic;
using UnityEngine;

namespace _COBRA_
{
    partial class Command
    {
        partial class Line
        {
            public bool TryReadPipe()
            {
                LintToThisPosition(Color.white);
                bool res = Util_cobra.TryReadPipe(text, ref read_i);
                if (res)
                    LintToThisPosition(linter.pipe);
                return res;
            }

            public bool HasNext(in bool save_move)
            {
                int read_i = this.read_i;
                Util_cobra.SkipCharactersUntil(text, ref read_i, true, false, Util_cobra.CHAR_SPACE);
                if (save_move)
                    this.read_i = read_i;
                return read_i < text.Length;
            }

            public void ReadBack()
            {
                --arg_i;
                read_i = start_i;
            }

            public bool TryReadArgument(out string argument, in IEnumerable<string> completions_candidates = null, in bool complete_if_is_option = false, in bool lint = true)
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    argument = string.Empty;
                    return false;
                }

                if (lint)
                    LintToThisPosition(linter._default_);

                Util_cobra.SkipCharactersUntil(text, ref read_i, true, false, Util_cobra.CHAR_SPACE);

                if (read_i > 0 && text[read_i - 1] != Util_cobra.CHAR_SPACE)
                {
                    argument = string.Empty;
                    return false;
                }

                start_i = read_i;
                Util_cobra.SkipCharactersUntil(text, ref read_i, true, true, Util_cobra.CHAR_SPACE, Util_cobra.CHAR_PIPE, Util_cobra.CHAR_CHAIN);

                bool isNotEmpty = false;

                if (start_i < read_i)
                {
                    arg_last = argument = text[start_i..read_i];
                    ++arg_i;

                    isNotEmpty = true;

                    if (lint)
                        LintToThisPosition(linter.argument);
                }
                else
                    argument = string.Empty;

                if (completions_candidates != null)
                    if (!complete_if_is_option || argument.StartsWith('-'))
                        if (signal.HasFlag(CMD_SIGNALS.CPL) && cursor_i >= start_i && cursor_i <= read_i)
                        {
                            cpl_start_i = read_i;
                            if (signal.HasFlag(CMD_SIGNALS.CPL_TAB))
                                ComputeCompletion_tab(argument, completions_candidates);
                            else if (signal.HasFlag(CMD_SIGNALS.CPL_ALT))
                                ComputeCompletion_alt(argument, completions_candidates);
                        }

                return isNotEmpty;
            }
        }
    }
}