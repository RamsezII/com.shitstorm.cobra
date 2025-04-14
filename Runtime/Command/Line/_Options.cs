using System.Collections.Generic;
using System;
using System.Linq;

namespace _COBRA_
{
    partial class Command
    {
        partial class Line
        {
            public bool TryRead_one_flag(in Executor executor, params string[] flags)
            {
                SkipLintToThisPosition();

                if (TryReadArgument(out string split, completions: flags, complete_if_option: true, strict: false, lint: false))
                    if (!split.StartsWith('-'))
                    {
                        ReadBack();
                        return false;
                    }
                    else if (flags.Contains(split, StringComparer.OrdinalIgnoreCase))
                    {
                        LintToThisPosition(linter.option);
                        return true;
                    }
                    else
                    {
                        ReadBack();
                        LintToThisPosition(linter.error);
                        executor.error = $"wrong flag '{split}'";
                        return false;
                    }
                return false;
            }

            public bool TryRead_one_of_the_flags(in Executor executor, out string output, params string[] flags)
            {
                output = string.Empty;
                HashSet<string> flags_remaining = new(flags, StringComparer.OrdinalIgnoreCase);

                SkipLintToThisPosition();

                if (TryReadArgument(out string split, flags_remaining, complete_if_option: true, strict: false, lint: false))
                    if (!split.StartsWith('-'))
                    {
                        ReadBack();
                        return false;
                    }
                    else if (!flags_remaining.Contains(split))
                    {
                        ReadBack();
                        LintToThisPosition(linter.error);
                        executor.error = $"wrong or already used option '{split}'";
                        return false;
                    }
                    else
                    {
                        LintToThisPosition(linter.option);
                        output = split;
                        return true;
                    }
                return false;
            }

            public bool TryRead_flags(in Executor executor, out HashSet<string> output, params string[] flags)
            {
                SkipLintToThisPosition();

                HashSet<string> flags_remaining = new(flags, StringComparer.OrdinalIgnoreCase);

                while (TryReadArgument(out string split, flags_remaining, complete_if_option: true, strict: false, lint: false))
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

            public bool TryRead_options(in Executor exe, out Dictionary<string, string> output, params string[] options)
            {
                SkipLintToThisPosition();

                HashSet<string> options_remaining = new(options, StringComparer.OrdinalIgnoreCase);
                output = new(options_remaining.Count, StringComparer.OrdinalIgnoreCase);

                while (TryReadArgument(out string split, options_remaining, complete_if_option: true, strict: false, lint: false))
                {
                    if (!split.StartsWith('-'))
                    {
                        ReadBack();
                        break;
                    }
                    cpl_stop = true;

                    if (!options.Contains(split))
                    {
                        LintToThisPosition(linter.error);
                        exe.error = $"wrong option '{split}'";
                        return false;
                    }
                    else if (!options_remaining.Contains(split))
                    {
                        LintToThisPosition(linter.error);
                        exe.error = $"option '{split}' already added";
                        return false;
                    }

                    LintToThisPosition(linter.option);
                    options_remaining.Remove(split);

                    if (TryReadArgument(out string opt_value))
                        output.Add(split, opt_value);
                    else
                    {
                        exe.error = $"option '{split}' requires a value";
                        LintToThisPosition(linter.error);
                    }
                }
                return true;
            }

            public bool TryRead_options_with_allowed_values(in Executor executor, in Dictionary<string, IEnumerable<string>> options, out Dictionary<string, string> output)
            {
                SkipLintToThisPosition();

                HashSet<string> options_remaining = new(options.Keys, StringComparer.OrdinalIgnoreCase);
                output = new(0, StringComparer.OrdinalIgnoreCase);

                while (TryReadArgument(out string split, options_remaining, complete_if_option: true, strict: false, lint: false))
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

                    if (TryReadArgument(out string arg, options[split], strict: false, lint: false))
                    {
                        LintToThisPosition(linter.option_value);
                        output[split] = arg;
                    }
                    else
                        return false;
                }
                return true;
            }

            public bool TryRead_options_parsed(in Executor executor, in Dictionary<string, Action<string>> options_parser)
            {
                SkipLintToThisPosition();

                HashSet<string> options_remaining = new(options_parser.Keys, StringComparer.OrdinalIgnoreCase);

                while (TryReadArgument(out string split, options_remaining, complete_if_option: true, strict: false, lint: false))
                {
                    if (!split.StartsWith('-'))
                    {
                        ReadBack();
                        break;
                    }
                    cpl_stop = true;

                    if (!options_parser.ContainsKey(split))
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
                    options_parser[split]?.Invoke(split);
                }
                return true;
            }
        }
    }
}