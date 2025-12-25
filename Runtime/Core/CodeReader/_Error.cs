using System.Text;
using UnityEngine;

namespace _COBRA_
{
    partial class CodeReader
    {
        public int sig_error_lineNumber;

        //----------------------------------------------------------------------------------------------------------

        public void CompilationError(in string error)
        {
            if (sig_error == null)
                if (sig_flags.HasFlag(SIG_FLAGS.SUBMIT))
                    Debug.LogError(error);
                else if (sig_flags.HasFlag(SIG_FLAGS.CHECK))
                    Debug.LogWarning(error);

            sig_error ??= error;
            err_trace ??= Util.GetStackTrace().GetFrame(1).ToString();
        }

        public void LocalizeError()
        {
            if (sig_long_error != null)
                return;

            sig_error_lineNumber = 1;
            int column_count = 0;
            int start_line = 0;
            string line = null;

            if (!multiline)
            {
                column_count = read_i;
                line = text;
            }
            else
                for (int i = 0; i < read_i; ++i)
                    switch (text[i])
                    {
                        case '\n':
                            ++sig_error_lineNumber;
                            column_count = 0;
                            break;

                        case '\r':
                            ++sig_error_lineNumber;
                            ++i;
                            column_count = 0;
                            break;

                        default:
                            if (column_count == 0)
                                start_line = i;
                            ++column_count;
                            break;
                    }

            if (multiline)
            {
                if (text.TryIndexOf_min(out int next_rn, start_line, true, '\r', '\n'))
                    line = text[start_line..next_rn];
                else
                    line = text[start_line..];

                StringBuilder sb = new();

                sb.AppendLine($"at {script_path ?? "line"}:{sig_error_lineNumber}");
                sb.AppendLine($"({nameof(last_arg)}: '{last_arg}', {sig_error_lineNumber}, {column_count})");
                sb.AppendLine($" {sig_error_lineNumber + ".",-4} {line}");
                sb.Append($"{new string(' ', 6 + column_count)}└──> {sig_error}");

                sig_long_error = sb.ToString();
            }
            else
                sig_long_error = $"{line}\n{new string(' ', read_i)}└──> {sig_error}";
            sig_long_error += "\n\n" + err_trace;
        }
    }
}