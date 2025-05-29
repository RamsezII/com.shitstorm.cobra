using System;
using System.Collections.Generic;
using _UTIL_;

namespace _COBRA_
{
    partial class Command
    {
        partial class Line
        {
            public class OptionParser : Dictionary<string, Func<Line, object>>
            {
                public static readonly Func<Line, object>
                    parser_workingdir = line =>
                    {
                        if (line.TryReadArgument(out string path, out bool seems_valid, path_mode: FS_TYPES.DIRECTORY))
                            if (seems_valid)
                                return path;
                        return null;
                    };

                //--------------------------------------------------------------------------------------------------------------

                public OptionParser(in bool ignore_case) : base(ignore_case ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal)
                {
                }
            }
        }
    }
}