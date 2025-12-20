using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _COBRA_
{
    partial class CodeReader
    {
        public bool TryLonguestMatch(out string match, in Color lint, in bool ignoreCase, in ICollection<string> matches, in string skip = _empties_)
        {
            var comp = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            int read_old = read_i;

            while (read_i < text.Length)
            {
                char c = text[read_i];
                if (!skip.Contains(c, comp))
                    break;
                ++read_i;
            }

            if (read_i < text.Length)
                foreach (string try_match in matches.OrderByDescending(x => x.Length))
                    if (text.StartsWith(try_match, comp))
                    {
                        bool contains_cursor = read_i <= cursor_i && read_i + try_match.Length >= cursor_i;

                        if (contains_cursor)
                            cpl_start = read_i;

                        read_i += try_match.Length;

                        if (contains_cursor)
                            cpl_end = read_i;

                        match = try_match;
                        LintToThisPosition(lint, true);
                        return true;
                    }

            read_i = read_old;
            match = null;
            return false;
        }
    }
}