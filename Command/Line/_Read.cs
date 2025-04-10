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
                text.SkipSpaces(ref read_i);
                LintToThisPosition(Color.white);
                bool res = Util_cobra.TryReadPipe(text, ref read_i);
                if (res)
                {
                    ++read_i;
                    LintToThisPosition(linter.pipe);
                }
                return res;
            }

            public bool HasNext(in bool save_move)
            {
                int read_i = this.read_i;
                text.SkipSpaces(ref read_i);
                if (save_move)
                    this.read_i = read_i;
                return read_i < text.Length;
            }

            public void ReadBack()
            {
                --arg_i;
                if (cpl_start_i == read_i)
                    cpl_done = false;
                end_i = read_i = start_i;
                text.GroupedErase(ref start_i);
                if (!cpl_done)
                    cpl_start_i = start_i;
            }

            public bool TryReadArgument(
                out string argument,
                in IEnumerable<string> completions_candidates = null,
                in bool complete_if_is_option = false,
                in bool lint = true)
            {
                if (lint)
                    LintToThisPosition(linter._default_);

                bool isNotEmpty = Util_cobra.TryReadArgument(text, out start_i, ref read_i, out argument, true);
                end_i = read_i;

                if (isNotEmpty)
                {
                    arg_last = argument;
                    ++arg_i;

                    if (lint)
                        LintToThisPosition(linter.argument);
                }

                //if (cpl_start_i == 0 || cpl_start_i != start_i)
                if (!cpl_done)
                    if (signal.HasFlag(CMD_SIGNALS.CPL))
                        if (cursor_i >= start_i && cursor_i <= read_i)
                            if (completions_candidates != null)
                                if (!complete_if_is_option || argument.StartsWith('-'))
                                {
                                    cpl_done = true;
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