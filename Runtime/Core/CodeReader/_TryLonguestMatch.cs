using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace _COBRA_
{
    partial class CodeReader
    {
        public bool TryReadArgument(out string argument, in bool ignoreCase, in string skips, in string stops, in Color lint)
        {
            var comp = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            int read_old = read_i;

            while (read_i < text.Length)
            {
                char c = text[read_i];
                if (!skips.Contains(c, comp))
                    break;
                ++read_i;
            }

            int start = read_i;
            while (read_i < text.Length)
            {
                char c = text[read_i];
                if (stops.Contains(c, comp))
                    break;
                ++read_i;
            }

            if (read_i > start)
            {
                argument = text[start..read_i];
                cpl_start = start;
                cpl_end = read_i;
                LintToThisPosition(lint, true);
                return true;
            }

            read_i = read_old;
            argument = null;
            return false;
        }

        public bool TryMatchLonguestCandidate(out string match, in bool ignoreCase, in Color lint, in ICollection<string> candidates, in string stops, in string skips = _empties_)
        {
            var comp = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            int read_old = read_i;

            if (TryReadArgument(out match, ignoreCase, skips, stops, lint))
            {
                if (cpl_start <= cursor_i && cursor_i <= read_i)
                    completions_v.UnionWith(candidates);

                foreach (string cand in candidates.OrderByDescending(x => x.Length))
                    if (match.StartsWith(cand, comp))
                    {
                        match = match[..cand.Length];
                        read_i = cpl_end = cpl_start + cand.Length;
                        LintToThisPosition(lint, true);
                        return true;
                    }
            }

            read_i = read_old;
            match = null;
            return false;
        }
    }
}