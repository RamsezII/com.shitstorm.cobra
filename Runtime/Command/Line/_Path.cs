﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using _UTIL_;

namespace _COBRA_
{
    partial class Command
    {
        partial class Line
        {
            public const string
                opt_workdir = "--working-directory",
                opt_wd = "-wd";

            //--------------------------------------------------------------------------------------------------------------

            public void TryReadOption_workdir(in Executor exe)
            {
                if (TryRead_one_of_the_flags(exe, out _, opt_wd, opt_workdir))
                    if (TryReadArgument(out string path, out bool seems_valid, path_mode: FS_TYPES.DIRECTORY))
                        if (seems_valid)
                            exe.opts.Add(opt_workdir, path);
            }

            public void LintPath()
            {
                string full_path = arg_last;
                try
                {
                    full_path = shell.PathCheck(full_path, PathModes.ForceFull);
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

            void PathCompletion_tab(string arg, in FS_TYPES flags, out IEnumerable<string> candidates)
            {
                string full_path = shell.PathCheck(arg, PathModes.ForceFull, out bool arg_rooted, out bool arg_in_workdir);

                if (arg.StartsWith("./", StringComparison.OrdinalIgnoreCase) || arg.StartsWith("..", StringComparison.OrdinalIgnoreCase))
                    arg = shell.PathCheck(arg, PathModes.TryLocal);

                string parent_dir = string.IsNullOrWhiteSpace(arg) ? shell.working_dir : Path.GetDirectoryName(full_path);
                if (!Directory.Exists(parent_dir))
                    parent_dir = shell.working_dir;

                candidates = flags switch
                {
                    FS_TYPES.DIRECTORY => Directory.EnumerateDirectories(parent_dir),
                    _ => Directory.EnumerateFileSystemEntries(parent_dir),
                };

                candidates = candidates.Select(path => shell.PathCheck(path, arg_rooted ? PathModes.ForceFull : PathModes.TryLocal));

                string[] array = ECompletionCandidates_tab(arg, candidates).ToArray();
                if (array.Length == 0)
                    return;
                InsertCompletionCandidate(array[cpl_index % array.Length]);
            }

            void PathCompletion_alt(in string arg, in FS_TYPES flags, out IEnumerable<string> candidates)
            {
                string full_path = shell.PathCheck(arg, PathModes.ForceFull, out bool arg_rooted, out bool arg_in_workdir);

                string parent_dir = string.IsNullOrWhiteSpace(arg) ? shell.working_dir : Path.GetDirectoryName(full_path);
                if (!Directory.Exists(parent_dir))
                    parent_dir = shell.working_dir;

                if (HasFlags_any(SIG_FLAGS.UP | SIG_FLAGS.DOWN))
                {
                    candidates = flags switch
                    {
                        FS_TYPES.DIRECTORY => Directory.EnumerateDirectories(parent_dir),
                        _ => Directory.EnumerateFileSystemEntries(parent_dir),
                    };

                    candidates = candidates.Select(path => shell.PathCheck(path, arg_rooted ? PathModes.ForceFull : PathModes.TryLocal));

                    string[] dirs = candidates.ToArray();

                    int indexOf = Array.IndexOf(dirs, arg);
                    if (indexOf == -1)
                        cpl_index = 0;
                    else
                        cpl_index = indexOf + this.flags switch
                        {
                            SIG_FLAGS f when f.HasFlag(SIG_FLAGS.UP) => -1,
                            SIG_FLAGS f when f.HasFlag(SIG_FLAGS.DOWN) => 1,
                            _ => 0,
                        };

                    while (cpl_index < 0)
                        cpl_index += dirs.Length;
                    cpl_index %= dirs.Length;

                    InsertCompletionCandidate(dirs[cpl_index]);
                }
                else if (this.flags.HasFlag(SIG_FLAGS.LEFT))
                    InsertCompletionCandidate(shell.PathCheck(parent_dir, arg_rooted ? PathModes.ForceFull : PathModes.TryLocal));
                else if (this.flags.HasFlag(SIG_FLAGS.RIGHT))
                {
                    candidates = flags switch
                    {
                        FS_TYPES.DIRECTORY => Directory.EnumerateDirectories(full_path),
                        _ => Directory.EnumerateFileSystemEntries(full_path),
                    };

                    foreach (string fs in candidates)
                    {
                        InsertCompletionCandidate(shell.PathCheck(fs, arg_rooted ? PathModes.ForceFull : PathModes.TryLocal));
                        break;
                    }
                }
                candidates = null;
            }
        }
    }
}