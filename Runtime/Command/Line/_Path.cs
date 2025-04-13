using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace _COBRA_
{
    partial class Command
    {
        partial class Line
        {
            public void LintPath()
            {
                string full_path = arg_last;
                try
                {
                    full_path = full_path.SafeRootedPath(shell.work_dir);
                }
                catch (Exception e)
                {
                    data = new(CMDLINE_STATUS.ERROR, e);
                }

                bool is_file = File.Exists(full_path);
                bool is_dir = !is_file && Directory.Exists(full_path);

                if (is_dir)
                    LintToThisPosition(linter.directory);
                else if (is_file)
                    LintToThisPosition(linter.file);
                else
                    LintToThisPosition(linter.path);
            }

            void PathCompletion_alt(in string arg)
            {
                string full_path = arg.SafeRootedPath(shell.work_dir);
                string parent_dir = Path.GetDirectoryName(full_path);

                if (HasFlags_any(SIGNALS.UP | SIGNALS.DOWN))
                {
                    string[] dirs =
                        Directory.EnumerateDirectories(parent_dir)
                        .Select(path => path.SafeRootedPath(shell.work_dir))
                        .ToArray();

                    int indexOf = Array.IndexOf(dirs, full_path);
                    if (indexOf >= 0)
                    {
                        cpl_index = indexOf + signal switch
                        {
                            SIGNALS s when s.HasFlag(SIGNALS.UP) => -1,
                            SIGNALS s when s.HasFlag(SIGNALS.DOWN) => 1,
                            _ => 0,
                        };

                        cpl_index %= dirs.Length;
                        if (cpl_index < 0)
                            cpl_index += dirs.Length;
                    }
                    else
                        cpl_index = 0;

                    InsertCompletionCandidate(dirs[cpl_index]);
                }
                else if (signal.HasFlag(SIGNALS.LEFT))
                    InsertCompletionCandidate(parent_dir);
                else if (signal.HasFlag(SIGNALS.RIGHT))
                    foreach (string fs in Directory.EnumerateFileSystemEntries(full_path))
                    {
                        InsertCompletionCandidate(fs.SafeRootedPath(shell.work_dir));
                        break;
                    }
            }
        }
    }
}