using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

            void PathCompletion_tab(in string arg, in PATH_FLAGS flags, out IEnumerable<string> candidates)
            {
                string full_path = arg.SafeRootedPath(shell.work_dir);
                string parent_dir = Path.GetDirectoryName(full_path);

                candidates = flags switch
                {
                    PATH_FLAGS.FILE => Directory.EnumerateFiles(parent_dir),
                    PATH_FLAGS.DIRECTORY => Directory.EnumerateDirectories(parent_dir),
                    _ => Directory.EnumerateDirectories(parent_dir),
                };

                candidates = candidates.Select(path => path.SafeRootedPath(shell.work_dir));

                string[] array = ECompletionCandidates_tab(arg, candidates).ToArray();
                if (array.Length == 0)
                    return;
                InsertCompletionCandidate(array[cpl_index % array.Length]);
            }

            void PathCompletion_alt(in string arg, in PATH_FLAGS flags, out IEnumerable<string> candidates)
            {
                string full_path = arg.SafeRootedPath(shell.work_dir);
                string parent_dir = Path.GetDirectoryName(full_path);

                if (HasFlags_any(SIGNALS.UP | SIGNALS.DOWN))
                {
                    candidates = flags switch
                    {
                        PATH_FLAGS.FILE => Directory.EnumerateFiles(parent_dir),
                        PATH_FLAGS.DIRECTORY => Directory.EnumerateDirectories(parent_dir),
                        _ => Directory.EnumerateDirectories(parent_dir),
                    };

                    candidates = candidates.Select(path => path.SafeRootedPath(shell.work_dir));

                    string[] dirs = candidates.ToArray();

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
                {
                    candidates = Directory.EnumerateFileSystemEntries(full_path);
                    foreach (string fs in candidates)
                    {
                        InsertCompletionCandidate(fs.SafeRootedPath(shell.work_dir));
                        break;
                    }
                }
                candidates = null;
            }
        }
    }
}