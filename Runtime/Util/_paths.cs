using _COBRA_;
using System;
using System.IO;

namespace _COBRA_
{
    public enum PathModes : byte
    {
        TryMaintain,
        TryLocal,
        ForceFull,
    }
}

partial class Util_cobra
{
    public static string PathCheck(in string workdir, in string path, in PathModes path_mode, in bool check_quotes, in bool force_quotes, out bool is_rooted, out bool isworkdirLocal)
    {
        bool empty = string.IsNullOrWhiteSpace(path);

        try
        {
            string result_path = path;

            if (empty)
            {
                is_rooted = false;
                isworkdirLocal = true;
                result_path = workdir;
            }
            else
            {
                is_rooted = Path.IsPathRooted(path);
                if (is_rooted)
                {
                    result_path = Path.GetFullPath(result_path).Replace("\\", "/");
                    isworkdirLocal = result_path.StartsWith(workdir, StringComparison.OrdinalIgnoreCase);
                }
                else
                {
                    isworkdirLocal = true;
                    result_path = Path.Combine(workdir, result_path);
                }
                result_path = Path.GetFullPath(result_path);
            }

            switch (path_mode)
            {
                case PathModes.TryMaintain when !is_rooted:
                case PathModes.TryLocal:
                    if (isworkdirLocal)
                        result_path = Path.GetRelativePath(workdir, result_path);
                    break;
            }

            result_path = result_path.Replace("\\", "/");

            if (force_quotes)
                result_path = result_path.QuoteStringSafely();
            else if (check_quotes)
                result_path = result_path.QuotePathIfNeeded();

            return result_path;
        }
        catch
        {
            is_rooted = false;
            isworkdirLocal = false;
            return path;
        }
    }
}