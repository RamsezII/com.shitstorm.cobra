using System;
using System.Text;
using UnityEngine;

namespace _COBRA_
{
    partial class Command
    {
        partial class Line
        {
            [Serializable]
            public class Linter
            {
                public int last_i;
                public bool fail_b;

                public readonly StringBuilder sb = new();

                public Color
                    _default_,
                    command,
                    argument,
                    option,
                    pipe,
                    chain,
                    background,
                    variable,
                    value,
                    type,
                    error;

                //--------------------------------------------------------------------------------------------------------------

                public string GetLint(in Executor exe, in string input)
                {
                    Line line = new(input, CMD_SIGNALS.LINT, exe.line.terminal, this);
                    exe.Executate(line);
                    line.EndLint(error);
                    string res = sb.PullValue();
                    Clear();
                    return res;
                }

                public void Clear()
                {
                    sb.Clear();
                    last_i = 0;
                    fail_b = false;
                }
            }

            //--------------------------------------------------------------------------------------------------------------

            public void EndLint(in Color color)
            {
                if (text.Length > linter.last_i)
                {
                    linter.sb.Append(text[linter.last_i..].SetColor(color));
                    linter.last_i = read_i;
                }
            }

            public void SkipLintToThisPosition()
            {
                if (read_i < linter.last_i)
                {
                    if (!linter.fail_b && false)
                        Debug.LogWarning($"{nameof(LintToThisPosition)}: {nameof(linter.last_i)}:{linter.last_i} < {nameof(linter)}.{nameof(linter.sb.Length)}:{linter.sb.Length}");

                    linter.fail_b = true;

                    linter.Clear();
                    linter.sb.Append(text[..read_i]);
                    return;
                }

                if (read_i > linter.last_i)
                {
                    linter.sb.Append(text[linter.last_i..read_i]);
                    linter.last_i = read_i;
                }
            }

            public void LintToThisPosition(in Color color)
            {
                if (read_i < linter.last_i)
                {
                    if (!linter.fail_b && false)
                        Debug.LogWarning($"{nameof(LintToThisPosition)}: {nameof(linter.last_i)}:{linter.last_i} < {nameof(linter)}.{nameof(linter.sb.Length)}:{linter.sb.Length}");

                    linter.fail_b = true;

                    linter.Clear();
                    linter.sb.Append(text[..read_i]);
                    return;
                }

                if (read_i > linter.last_i)
                {
                    linter.sb.Append(text[linter.last_i..read_i].SetColor(color));
                    linter.last_i = read_i;
                }
            }
        }
    }
}