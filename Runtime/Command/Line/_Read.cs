using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace _COBRA_
{
    partial class Command
    {
        partial class Line
        {
            public bool TryReadArguments(out string arguments)
            {
                LintToThisPosition(linter._default_);
                if (text.TryReadArguments(out _, ref read_i, out arguments))
                {
                    LintToThisPosition(linter.external);
                    return true;
                }
                return false;
            }

            public bool TryReadCommandSeparator(out string argument)
            {
                if (!HasNext(true))
                {
                    argument = string.Empty;
                    return false;
                }

                LintToThisPosition(linter._default_);

                int start_i = read_i;
                char c = text[read_i];

                switch (c)
                {
                    case '&':
                        ++read_i;
                        if (read_i < text.Length && text[read_i] == '&')
                        {
                            ++read_i;
                            LintToThisPosition(linter.chain);
                        }
                        else
                            LintToThisPosition(linter.background);
                        argument = text[start_i..read_i];
                        return true;

                    //case '>':
                    //    ++read_i;
                    //    if (read_i < text.Length && text[read_i] == '>')
                    //    {
                    //        ++read_i;
                    //        LintToThisPosition(linter.redirect_overwrite);
                    //    }
                    //    else
                    //        LintToThisPosition(linter.redirect_append);
                    //    argument = text[start_i..read_i];
                    //    return true;

                    case '|':
                        ++read_i;
                        LintToThisPosition(linter.pipe);
                        argument = text[start_i..read_i];
                        return true;

                    default:
                        argument = string.Empty;
                        return false;
                }
            }

            public bool HasNext(in bool save_move, in bool lint = true)
            {
                int read_i = this.read_i;
                text.SkipSpaces(ref read_i);

                if (lint)
                    if (linter == null)
                        LintToThisPosition(Color.gray);
                    else
                        LintToThisPosition(linter._default_);

                if (save_move)
                    this.read_i = read_i;

                return read_i < text.Length;
            }

            public void ReadBack()
            {
                --arg_i;

                if (cpl_start_i == read_i)
                {
                    cpl_done = false;
                    is_cursor_on_path = false;
                }

                if (start_i > 0 && text[start_i - 1] == '@')
                    --start_i;

                end_i = read_i = start_i;
                text.GroupedErase(ref start_i);

                if (!cpl_done)
                    cpl_start_i = start_i;
            }

            public bool TryReadArgument(
                out string argument,
                out bool seems_valid,
                IEnumerable<string> completions = null,
                in bool complete_if_option = false,
                in bool strict = false,
                in PATH_FLAGS path_mode = 0,
                in bool stop_if_var = false,
                in bool lint = true)
            {
                if (lint)
                    LintToThisPosition(linter._default_);

                bool had_next = HasNext(true, lint);
                bool is_var = had_next && text[read_i] == '@';

                if (is_var)
                {
                    if (stop_if_var)
                    {
                        argument = string.Empty;
                        seems_valid = false;
                        return false;
                    }

                    completions = shell.shell_vars.Keys.Concat(Shell.global_vars.Keys).Select(var_name => $"@{var_name}");
                }

                seems_valid = false;

                bool isNotEmpty = Util_cobra.TryReadArgument(text, out start_i, ref read_i, out argument, stop_at_separators: true);
                end_i = read_i;

                string var_name = is_var && argument.Length > 1
                    ? argument[1..]
                    : argument;

                string var_value_str = is_var ? string.Empty : argument;
                if (is_var)
                    if (shell.shell_vars.TryGetValue(var_name, out var var_value) || Shell.global_vars.TryGetValue(var_name, out var_value))
                    {
                        var_value_str = var_value.ToString();
                        seems_valid = true;
                    }
                    else if (HasFlags_any(SIGNALS.CHECK | SIGNALS.EXEC | SIGNALS.TICK))
                        Debug.LogWarning($"no var named: '{var_name}'");

                if (isNotEmpty)
                {
                    arg_last = var_name;
                    ++arg_i;

                    if (lint)
                        if (path_mode == 0)
                            if (is_var)
                            {
                                if (shell.shell_vars.ContainsKey(var_name))
                                    LintToThisPosition(linter.var_shell);
                                else if (Shell.global_vars.ContainsKey(var_name))
                                    LintToThisPosition(linter.var_global);
                                else
                                    LintToThisPosition(linter.var_unknown);
                            }
                            else
                                LintToThisPosition(linter.argument);
                        else
                        {
                            is_cursor_on_path = IsOnCursor;
                            if (is_cursor_on_path)
                            {
                                path_i = start_i;
                                path_last = arg_last;
                            }
                            LintPath();
                        }
                }

                if (!cpl_done)
                    if (signal.HasFlag(SIGNALS.CPL))
                        if (IsOnCursor)
                            if (path_mode.HasFlags_any(PATH_FLAGS.BOTH) || completions != null)
                                if (!complete_if_option || argument.StartsWith('-'))
                                {
                                    cpl_done = true;
                                    cpl_start_i = read_i;
                                    if (signal.HasFlag(SIGNALS.CPL_TAB))
                                        if (path_mode.HasFlags_any(PATH_FLAGS.BOTH))
                                            PathCompletion_tab(var_value_str, path_mode, out completions);
                                        else
                                            ComputeCompletion_tab(var_value_str, completions);
                                    else if (signal.HasFlag(SIGNALS.CPL_ALT))
                                        if (path_mode.HasFlags_any(PATH_FLAGS.BOTH))
                                            PathCompletion_alt(var_value_str, path_mode, out completions);
                                        else
                                            ComputeCompletion_alt(var_value_str, completions);
                                }

                if (!is_var)
                    if (path_mode == 0)
                        seems_valid = completions != null && completions.Contains(var_value_str);
                    else
                    {
                        string full_path = shell.PathCheck(argument, PathModes.ForceFull);
                        if (path_mode.HasFlag(PATH_FLAGS.FILE) && File.Exists(full_path))
                            seems_valid = true;
                        else if (path_mode.HasFlag(PATH_FLAGS.DIRECTORY) && Directory.Exists(full_path))
                            seems_valid = true;
                        else
                            seems_valid = false;
                    }

                argument = var_value_str;

                if (strict && !seems_valid)
                {
                    if (had_next)
                        ReadBack();
                    else
                    {
                        if (cpl_start_i == start_i)
                            cpl_done = false;
                    }
                    return false;
                }

                return isNotEmpty;
            }
        }
    }
}