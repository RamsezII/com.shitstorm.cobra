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
                BoaProgram program = new(reader);
                if (reader.sig_flags.HasFlag(SIG_FLAGS.SUBMIT))
                    if (reader.sig_error != null)
                    {
                        reader.LocalizeError();
                        Debug.LogError(reader.sig_long_error);
                        status.Value = CMD_STATUS.WAIT_FOR_STDIN;
                    }
                    else
                    {
                        front_janitor = new(this, program.asts);
                        status.Value = CMD_STATUS.BLOCKED;
                    }
            }
        }

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

                    if (!front_janitor.Disposed)
                        status.Value = output.status;
                    else
                    {
                        front_janitor = null;
                        status.Value = CMD_STATUS.WAIT_FOR_STDIN;
                    }
                }
        }
    }
}