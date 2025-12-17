using _UTIL_;
using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace _COBRA_
{
    partial class CodeReader
    {
        public bool TryParsePath(in FS_TYPES type, in bool read_as_argument, out string path)
        {
            if (!TryParseString(out path, read_as_argument))
                if (sig_error == null)
                    TryReadArgument(out path, false, lint_theme.strings, stoppers: _stoppers_paths);

            if (sig_error != null)
                goto failure;

            int read_old = read_i;
            HasNext();
            if (!IsOnCursor())
                read_i = read_old;
            else
            {
                stop_completing = true;
                completion_l = completion_r = null;

                try
                {
                    if (string.IsNullOrWhiteSpace(path))
                        path = "./";

                    string long_path = Util_cobra.PathCheck(workdir, path, PathModes.ForceFull, false, false, out bool is_rooted, out bool is_local_to_shell);
                    bool ends_with_bar = long_path.EndsWith('/', '\\');

                    if (ends_with_bar)
                        long_path = long_path[..^1];

                    PathModes path_mode = is_rooted ? PathModes.ForceFull : PathModes.TryLocal;
                    DirectoryInfo parent = ends_with_bar ? new(long_path) : Directory.GetParent(long_path);

                    if (parent != null)
                    {
                        completion_l = Util_cobra.PathCheck(workdir, ends_with_bar ? long_path : parent.FullName, path_mode, true, true, out _, out _);

                        if (parent.Exists)
                        {
                            if (!ends_with_bar)
                                completion_r = (Util_cobra.PathCheck(workdir, long_path, path_mode, false, false, out _, out _) + "/").QuoteStringSafely();
                            else
                            {
                                DirectoryInfo current = new(long_path);
                                if (current != null && current.Exists)
                                {
                                    string path_r;
                                    if (type == FS_TYPES.DIRECTORY)
                                        path_r = current.EnumerateDirectories().FirstOrDefault()?.FullName ?? long_path;
                                    else
                                        path_r = current.EnumerateFileSystemInfos().FirstOrDefault()?.FullName ?? long_path;
                                    completion_r = Util_cobra.PathCheck(workdir, path_r, path_mode, true, true, out _, out _);
                                }
                            }

                            if (sig_flags.HasFlag(SIG_FLAGS.CHANGE))
                            {
                                var paths = type switch
                                {
                                    FS_TYPES.DIRECTORY => parent.EnumerateDirectories(),
                                    _ => parent.EnumerateFileSystemInfos(),
                                };

                                foreach (var dir in parent.EnumerateDirectories())
                                    completions_v.Add(Util_cobra.PathCheck(workdir, dir.FullName, path_mode, true, true, out _, out _));

                                if (type.HasFlag(FS_TYPES.FILE))
                                    foreach (var file in parent.EnumerateFiles())
                                        completions_v.Add(Util_cobra.PathCheck(workdir, file.FullName, path_mode, true, true, out _, out _));
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            return true;

        failure:
            path = null;
            CompilationError($"could not parse path '{path}'.");
            return false;
        }
    }
}