﻿using System;
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

                public Color _default_ = Color.gray;
                public Color _readall_ = Color.gray;
                public Color command = Color.gray;
                public Color argument = Color.gray;
                public Color option = Color.gray;
                public Color option_value = Color.gray;
                public Color pipe = Color.gray;
                public Color chain = Color.gray;
                public Color redirect_overwrite = Color.gray;
                public Color redirect_append = Color.gray;
                public Color external = Color.gray;
                public Color background = Color.gray;
                public Color var_global = Color.gray;
                public Color var_shell = Color.gray;
                public Color var_local = Color.gray;
                public Color var_unknown = Color.gray;
                public Color literal = Color.gray;
                public Color value = Color.gray;
                public Color boa_symbol = Color.gray;
                public Color type = Color.gray;
                public Color directory = Color.gray;
                public Color file = Color.gray;
                public Color path = Color.gray;
                public Color bundle = Color.gray;
                public Color asset = Color.gray;
                public Color error = Color.gray;

                //--------------------------------------------------------------------------------------------------------------

                public string GetLint(in Shell shell, in string input, out Line line, in SIG_FLAGS flags = 0, in int cursor_i = -1)
                {
                    line = new(input, SIG_FLAGS.LINT | flags, shell, cursor_i: cursor_i);
                    shell.PropagateSignal(line);
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

            public void NoLintNoRead()
            {
                linter.last_i = read_i = text.Length;
                linter.sb.Clear();
                linter.sb.Append(text);
            }

            public void EndLint(in Color color)
            {
                if (!flags.HasFlag(SIG_FLAGS.LINT))
                    return;

                if (string.IsNullOrEmpty(text))
                    return;

                if (text.Length > linter.last_i)
                {
                    linter.sb.Append(text[linter.last_i..].SetColor(color));
                    linter.last_i = read_i;
                }
            }

            public void SkipLintToThisPosition()
            {
                if (!flags.HasFlag(SIG_FLAGS.LINT))
                    return;

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
                if (!flags.HasFlag(SIG_FLAGS.LINT))
                    return;

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

            public void LintToThisPosition(in int length, in string text_linted)
            {
                linter.last_i += length;
                linter.sb.Append(text_linted);
            }
        }
    }
}