using _COBRA_.Boa;
using System.Collections.Generic;
using UnityEngine;

namespace _COBRA_
{
    public sealed class BoaShell : Shell
    {
        /*

            Instruction
            └── Expression
                └── Assignation (=)
                    └── Conditional (ternary ? :)
                        └── Or (||)
                            └── And (&&)
                                └── Comparison
                                    └── Addition (+ -)
                                        └── Term (* / %)
                                            └── Unary (! - ~)
                                                └── Factor

        */

        readonly List<Janitor> janitors = new();
        Janitor front_janitor;
        internal readonly MemScope scope = new();

        //----------------------------------------------------------------------------------------------------------

        public override void Init()
        {
            base.Init();
            status.Value = CMD_STATUS.WAIT_FOR_STDIN;
        }

        //----------------------------------------------------------------------------------------------------------

        public override void OnReader(in CodeReader reader)
        {
            if (front_janitor != null)
            {
                front_janitor.OnReader(reader, out ExecutionOutput output);

                if (output.status == CMD_STATUS.ERROR)
                {
                    Debug.LogError($"{this} SIG_ERROR['{reader.sig_flags}']: \"{output.error}\"");
                    front_janitor.Dispose();
                }

                if (!front_janitor.Disposed)
                    status.Value = output.status;
                else
                {
                    front_janitor = null;
                    status.Value = CMD_STATUS.WAIT_FOR_STDIN;
                }
            }
            else
            {
                Queue<AstAbstract> asts = new();

                MemScope scope = new(this.scope);

                while (reader.HasNext() && AstStatement.TryStatement(reader, scope, out var ast))
                    if (ast != null)
                        asts.Enqueue(ast);

                bool execute_in_background = reader.TryReadChar_match('&', lint: reader.lint_theme.command_separators);

                if (reader.TryPeekChar_out(out char peek, out _))
                    reader.CompilationError($"could not parse everything ({nameof(peek)}: '{peek}').");

                if (reader.sig_flags.HasFlag(SIG_FLAGS.SUBMIT))
                    if (reader.sig_error != null)
                    {
                        reader.LocalizeError();
                        Debug.LogError(reader.sig_long_error);
                        status.Value = CMD_STATUS.WAIT_FOR_STDIN;
                    }
                    else if (execute_in_background)
                        janitors.Add(new(this, asts));
                    else
                    {
                        front_janitor = new(this, asts);
                        status.Value = CMD_STATUS.BLOCKED;
                    }
            }
        }

        //----------------------------------------------------------------------------------------------------------

        protected override void OnTick()
        {
            for (int i = 0; i < janitors.Count; i++)
            {
                Janitor janitor = janitors[i];
                janitor.OnTick(out ExecutionOutput output);

                if (output.status == CMD_STATUS.ERROR)
                {
                    Debug.LogError($"{this} TICK_ERROR_bg: \"{output.error}\"");
                    janitor.Dispose();
                }

                if (janitor.Disposed)
                    janitors.RemoveAt(i--);
            }

            if (front_janitor != null)
                if (!front_janitor.Disposed)
                {
                    front_janitor.OnTick(out ExecutionOutput output);

                    if (output.status == CMD_STATUS.ERROR)
                    {
                        Debug.LogError($"{this} TICK_ERROR_bg: \"{output.error}\"");
                        front_janitor.Dispose();
                    }

                    if (front_janitor.Disposed)
                    {
                        front_janitor = null;
                        status.Value = CMD_STATUS.WAIT_FOR_STDIN;
                    }
                    else
                        status.Value = output.status;
                }
        }
    }
}