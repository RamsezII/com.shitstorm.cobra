using _UTIL_;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace _COBRA_.Boa
{
    public sealed class Janitor : Disposable
    {
        public readonly BoaShell shell;
        readonly Queue<AstAbstract> asts;

        public readonly MemStack vstack = new();
        internal readonly Queue<Executor> executors = new();

        internal CodeReader reader;

        readonly IEnumerator<ExecutionStatus> routine;

        //----------------------------------------------------------------------------------------------------------

        public Janitor(in string name, in BoaShell shell, in Queue<AstAbstract> asts):base($"{shell.name}->{name}")
        {
            this.shell = shell;
            this.asts = asts;
            routine = ERoutine();
        }

        //----------------------------------------------------------------------------------------------------------

        IEnumerator<ExecutionStatus> ERoutine()
        {
            while (asts.TryDequeue(out var ast))
            {
                ast.OnExecutorsQueue(vstack, shell.scope, executors);

                while (executors.TryDequeue(out var executor))
                {
                    if (executor.Disposed)
                        continue;

                    try
                    {
                        if (executor.routine_SIG_READER != null)
                        {
                            using var routine = executor.routine_SIG_READER(this);

                            while (!executor.Disposed && routine.MoveNext())
                                yield return routine.Current;
                        }

                        if (executor.Disposed)
                            continue;

                        if (executor.action_SIG_EXE != null)
                        {
                            while (!executor.Disposed && reader != null)
                                yield return default;

                            if (!executor.Disposed)
                                executor.action_SIG_EXE();
                        }

                        if (executor.Disposed)
                            continue;

                        if (executor.routine_SIG_EXE != null)
                        {
                            while (!executor.Disposed && reader != null)
                                yield return default;

                            using var routine = executor.routine_SIG_EXE();

                            while (!executor.Disposed)
                                if (reader != null)
                                    yield return default;
                                else if (routine.MoveNext())
                                    yield return routine.Current;
                                else
                                    break;
                        }
                    }
                    finally
                    {
                        executor.Dispose();
                    }
                }
            }
        }

        public bool OnReader(in CodeReader reader, out ExecutionStatus output)
        {
            this.reader = reader;
            try
            {
                return OnTick(out output);
            }
            finally
            {
                this.reader = null;
            }
        }

        public bool OnTick(out ExecutionStatus output)
        {
            if (!Disposed)
            {
                int loops = 0;
            again:
                try
                {
                    if (routine.MoveNext())
                    {
                        if (routine.Current.code == CMD_STATUS.RETURN)
                            if (++loops < 100)
                                goto again;

                        output = routine.Current;
                        return true;
                    }

                    Dispose();
                }
                catch (Exception exception)
                {
                    output = new(CMD_STATUS.ERROR, error: exception.ToString());
                    Dispose();
                    return true;
                }
            }
            output = default;
            return false;
        }

        //----------------------------------------------------------------------------------------------------------

        protected override void OnDispose()
        {
            base.OnDispose();

            if (vstack.Count > 0)
                Debug.LogWarning($"{this} disposed of {vstack.Count} unused memory cells...");

            routine?.Dispose();

            while (executors.TryDequeue(out Executor executor))
                executor.Dispose();

            asts.Clear();
            vstack.Clear();
        }
    }
}
