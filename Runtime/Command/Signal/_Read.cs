using System.Collections.Generic;
using System.IO;
using System.Linq;
using _UTIL_;
using UnityEngine;

namespace _COBRA_
{
    partial class Command
    {
        partial class Signal
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

            public bool TryReadCommandSeparator(out string argument, in bool is_pipe = false)
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
                    case '&' when !is_pipe:
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

                    case '>':
                        ++read_i;
                        if (read_i < text.Length && text[read_i] == '>')
                        {
                            ++read_i;
                            LintToThisPosition(linter.redirect_overwrite);
                        }
                        else
                            LintToThisPosition(linter.redirect_append);
                        argument = text[start_i..read_i];
                        return true;

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
                in FS_TYPES path_mode = 0,
                in bool stop_if_var = false,
                in bool lint = true,
                in bool find_and_replace_variables = true)
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
                    else
                    {
                        var_value_str = string.Empty;
                        if (HasFlags_any(SIG_FLAGS.EXEC | SIG_FLAGS.TICK))
                            Debug.LogWarning($"no var named: '{var_name}'");
                    }

                if (!is_var)
                    if (isNotEmpty)
                        if (find_and_replace_variables)
                            for (int i = 0; i < var_value_str.Length; ++i)
                            {
                                char c = var_value_str[i];
                                if (c == '{')
                                {
                                    int vi = i;
                                    while (i < var_value_str.Length - 1 && var_value_str[++i] != '}') ;
                                    if (i > vi + 1)
                                    {
                                        string varname = var_value_str[(vi + 1)..i];
                                        if (shell.shell_vars.TryGetValue(varname, out var var_value) || Shell.global_vars.TryGetValue(varname, out var_value))
                                        {
                                            string val = var_value.ToString();
                                            var_value_str = var_value_str[..vi] + val + var_value_str[(i + 1)..];
                                            i = vi + val.Length;
                                        }
                                    }
                                }
                            }

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
                    if (flags.HasFlag(SIG_FLAGS.CPL))
                        if (IsOnCursor)
                            if (path_mode.HasFlags_any(FS_TYPES.BOTH) || completions != null)
                                if (!complete_if_option || argument.StartsWith('-'))
                                {
                                    cpl_done = true;
                                    cpl_last_i = cpl_start_i = read_i;

                                    if (flags.HasFlag(SIG_FLAGS.ALT))
                                        if (path_mode.HasFlags_any(FS_TYPES.BOTH))
                                            PathCompletion_alt(var_value_str, path_mode, out completions);
                                        else
                                            ComputeCompletion_alt(var_value_str, completions);
                                    else if (HasFlags_any(SIG_FLAGS.CPL))
                                    {
                                        if (path_mode.HasFlags_any(FS_TYPES.BOTH))
                                            PathCompletion_tab(var_value_str, path_mode, out completions);
                                        else
                                            ComputeCompletion_tab(var_value_str, ref completions);
                                        this.completions = completions;
                                    }
                                }

                if (!is_var)
                    if (path_mode == 0)
                        seems_valid = completions != null && completions.Contains(var_value_str);
                    else
                    {
                        string full_path = shell.PathCheck(argument, PathModes.ForceFull);
                        if (path_mode.HasFlag(FS_TYPES.FILE) && File.Exists(full_path))
                            seems_valid = true;
                        else if (path_mode.HasFlag(FS_TYPES.DIRECTORY) && Directory.Exists(full_path))
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