using _ARK_;
using _COBRA_.Boa;
using System;
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

        public readonly List<Janitor> background_janitors = new();
        public Janitor front_janitor;
        public readonly MemScope scope;

        //----------------------------------------------------------------------------------------------------------

        public BoaShell(in string name) : base(name)
        {
            scope = new("root_scope", this);
        }

        //----------------------------------------------------------------------------------------------------------

        void ReassignGlobalVars()
        {
            scope._vars["_time"] = Time.time;
            scope._vars["_ftime"] = Time.fixedTime;
            scope._vars["_dtime"] = Time.deltaTime;
            scope._vars["_frame"] = Time.frameCount;
            scope._vars["_fframe"] = NUCLEOR.instance.fixedFrameCount;
        }

        protected override void OnTick()
        {
            ReassignGlobalVars();

            for (int i = 0; i < background_janitors.Count; i++)
            {
                Janitor janitor = background_janitors[i];
                janitor.OnTick(out ExecutionStatus output);

                if (output.code == CMD_STATUS.ERROR)
                {
                    Debug.LogError($"{this} TICK_ERROR_bg: \"{output.error}\"");
                    janitor.Dispose();
                }

                if (janitor.Disposed)
                    background_janitors.RemoveAt(i--);
            }

            if (front_janitor != null)
                if (!front_janitor.Disposed)
                {
                    front_janitor.OnTick(out ExecutionStatus status);

                    if (status.code == CMD_STATUS.ERROR)
                    {
                        Debug.LogError($"{this} TICK_ERROR_bg: \"{status.error}\"");
                        front_janitor.Dispose();
                    }

                    if (!front_janitor.Disposed)
                        this.status.Value = status;
                    else
                    {
                        front_janitor = null;
                        this.status.Value = RegularStatus();
                    }
                }
        }

        public override void OnReader(in CodeReader reader)
        {
            ReassignGlobalVars();

            if (front_janitor != null)
            {
                front_janitor.OnReader(reader, out ExecutionStatus status);

                if (status.code == CMD_STATUS.ERROR)
                {
                    Debug.LogError($"{this} SIG_ERROR['{reader.sig_flags}']: \"{status.error}\"");
                    front_janitor.Dispose();
                }

                if (!front_janitor.Disposed)
                    this.status.Value = status;
                else
                {
                    front_janitor = null;
                    this.status.Value = RegularStatus();
                }
            }
            else
            {
                Queue<AstAbstract> asts = new();

                MemScope ast_scope = reader.sig_flags.HasFlag(SIG_FLAGS.SUBMIT)
                    ? scope
                    : scope.GetSubScope("ast_scope");

                while (reader.HasNext() && AstStatement.TryStatement(reader, ast_scope, out var ast))
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
                        status.Value = RegularStatus();
                    }
                    else if (execute_in_background)
                        background_janitors.Add(new("background_janitor", this, asts));
                    else
                    {
                        front_janitor = new("front_janitor", this, asts);
                        status.Value = default;
                    }
            }
        }
    }
}