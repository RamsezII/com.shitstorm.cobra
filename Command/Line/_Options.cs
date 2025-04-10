using System.Collections.Generic;
using System;
using System.Linq;

namespace _COBRA_
{
    partial class Command
    {
        partial class Line
        {
            public bool TryReadOneOfFlags(in Executor executor, out string output, params string[] flags)
            {
                SkipLintToThisPosition();

                HashSet<string> flags_remaining = new(flags, StringComparer.OrdinalIgnoreCase);

                if (TryReadArgument(out string split, flags_remaining, complete_if_is_option: true, accept_only_candidate: false, lint: false))
                    if (!split.StartsWith('-'))
                        ReadBack();
                    else if (!flags_remaining.Contains(split))
                    {
                        LintToThisPosition(linter.error);
                        executor.error = $"wrong or already used option '{split}'";
                    }
                    else
                    {
                        LintToThisPosition(linter.option);
                        output = split;
                        return true;
                    }

                output = string.Empty;
                return true;
            }

            public bool TryReadFlags(in Executor executor, out HashSet<string> output, params string[] flags)
            {
                SkipLintToThisPosition();

                HashSet<string> flags_remaining = new(flags, StringComparer.OrdinalIgnoreCase);

                while (TryReadArgument(out string split, flags_remaining, complete_if_is_option: true, accept_only_candidate: false, lint: false))
                {
                    if (!split.StartsWith('-'))
                    {
                        ReadBack();
                        break;
                    }

                    if (!flags_remaining.Contains(split))
                    {
                        LintToThisPosition(linter.error);

                        executor.error = $"wrong or already used option '{split}'";
                        output = null;
                        return false;
                    }

                    LintToThisPosition(linter.option);
                    flags_remaining.Remove(split);
                }

                output = new(flags.Except(flags_remaining), StringComparer.OrdinalIgnoreCase);
                return true;
            }

            public bool TryReadOptions(in Executor executor, in Dictionary<string, Action<string>> onOption)
            {
                SkipLintToThisPosition();

                HashSet<string> options_remaining = new(onOption.Keys, StringComparer.OrdinalIgnoreCase);

                while (TryReadArgument(out string split, options_remaining, complete_if_is_option: true, accept_only_candidate: false, lint: false))
                {
                    if (!split.StartsWith('-'))
                    {
                        ReadBack();
                        break;
                    }
                    cpl_stop = true;

                    if (!onOption.ContainsKey(split))
                    {
                        LintToThisPosition(linter.error);
                        executor.error = $"wrong option '{split}'";
                        return false;
                    }
                    else if (!options_remaining.Contains(split))
                    {
                        LintToThisPosition(linter.error);
                        executor.error = $"option '{split}' already added";
                        return false;
                    }

                    LintToThisPosition(linter.option);
                    options_remaining.Remove(split);
                    onOption[split]?.Invoke(split);
                }
                return true;
            }

            public bool TryReadOptions(in Executor executor, in Dictionary<string, IEnumerable<string>> options, out Dictionary<string, string> output)
            {
                SkipLintToThisPosition();

                HashSet<string> options_remaining = new(options.Keys, StringComparer.OrdinalIgnoreCase);
                output = new(0, StringComparer.OrdinalIgnoreCase);

                while (TryReadArgument(out string split, options_remaining, complete_if_is_option: true, accept_only_candidate: false, lint: false))
                {
                    if (!split.StartsWith('-'))
                    {
                        ReadBack();
                        break;
                    }

                    if (!options.ContainsKey(split))
                    {
                        LintToThisPosition(linter.error);
                        executor.error = $"wrong option '{split}'";
                        return false;
                    }
                    else if (!options_remaining.Contains(split))
                    {
                        LintToThisPosition(linter.error);
                        executor.error = $"option '{split}' already added";
                        return false;
                    }

                    LintToThisPosition(linter.option);
                    options_remaining.Remove(split);

                    if (TryReadArgument(out string arg, options[split], accept_only_candidate: false, lint: false))
                    {
                        LintToThisPosition(linter.option_value);
                        output[split] = arg;
                    }
                    else
                        return false;
                }
                return true;
            }
        }
    }
}