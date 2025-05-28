using System.Collections.Generic;
using System;
using System.Linq;

namespace _COBRA_
{
    partial class Command
    {
        partial class Signal
        {
            void InsertCompletionCandidate(string candidate)
            {
                if (candidate.Contains(' '))
                    candidate = candidate.QuoteStringSafely();

                text = text[..start_i] + candidate + text[read_i..];
                cursor_i = read_i = start_i + candidate.Length;
            }

            void ComputeCompletion_tab(in string argument, in IEnumerable<string> candidates)
            {
                string[] array = ECompletionCandidates_tab(argument, candidates).ToArray();
                if (array.Length == 0)
                    return;
                InsertCompletionCandidate(array[cpl_index % array.Length]);
            }

            IEnumerable<string> ECompletionCandidates_tab(string argument, IEnumerable<string> candidates)
            {
                foreach (string candidate in candidates)
                {
                    int last = 0, ic = 0, matches = 0;
                    while (ic < argument.Length)
                    {
                        int i = candidate.IndexOf(argument[ic..++ic], last, candidate.Length - last, StringComparison.OrdinalIgnoreCase);
                        if (i >= 0)
                        {
                            last = i + 1;
                            ++matches;
                        }
                    }
                    if (matches >= argument.Length)
                        yield return candidate;
                }
            }

            void ComputeCompletion_alt(in string argument, in IEnumerable<string> candidates)
            {
                IList<string> list = candidates as IList<string> ?? candidates.ToArray();
                if (list.Count == 0)
                    return;

                int indexOf = list.IndexOf(argument);
                if (indexOf >= 0)
                {
                    cpl_index = indexOf + flags switch
                    {
                        SIG_FLAGS.ALT_UP => -1,
                        SIG_FLAGS.ALT_DOWN => 1,
                        _ => 0,
                    };

                    cpl_index %= list.Count;
                    if (cpl_index < 0)
                        cpl_index += list.Count;
                }
                else
                    cpl_index = 0;

                InsertCompletionCandidate(list[cpl_index]);
            }
        }
    }
}