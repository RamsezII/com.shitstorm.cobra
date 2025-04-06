using System.Collections.Generic;
using System;
using System.Linq;

namespace _COBRA_
{
    partial class Command
    {
        partial class Line
        {
            public bool TryReadFlags(in Executor executor, out HashSet<string> output, params string[] flags)
            {
                SkipLintToThisPosition();

                HashSet<string> flags_remaining = new(flags, StringComparer.OrdinalIgnoreCase);

                while (TryReadArgument(out string split, flags_remaining, complete_if_is_option: true, lint: false))
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

                while (TryReadArgument(out string split, options_remaining, complete_if_is_option: true, lint: false))
                {
                    if (!split.StartsWith('-'))
                    {
                        ReadBack();
                        break;
                    }

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
        }
    }
}