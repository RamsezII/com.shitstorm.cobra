using _UTIL_;
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

        public Janitor(in BoaShell shell, in Queue<AstAbstract> asts)
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
            before_executor:
                while (executors.TryDequeue(out var executor))
                    if (!executor.Disposed)
                    {
                        if (executor.routine_SIG_READER != null)
                        {
                            while (true)
                                if (executor.Disposed)
                                    goto before_executor;
                                else
                                    break;

                            if (executor.Disposed)
                                goto before_executor;

                            using var routine = executor.routine_SIG_READER(this);

                            while (true)
                                if (executor.Disposed)
                                    goto before_executor;
                                else if (routine.MoveNext())
                                    yield return routine.Current;
                                else
                                    break;
                        }

                        if (executor.action_SIG_EXE != null)
                        {
                            while (true)
                                if (executor.Disposed)
                                    goto before_executor;
                                else if (reader != null)
                                    yield return default;
                                else
                                    break;

                            if (!executor.Disposed)
                                executor.action_SIG_EXE();
                        }

                        if (executor.routine_SIG_EXE != null)
                        {
                            while (true)
                                if (executor.Disposed)
                                    goto before_executor;
                                else if (reader != null)
                                    yield return default;
                                else
                                    break;

                            if (executor.Disposed)
                                goto before_executor;

                            using var routine = executor.routine_SIG_EXE();

                            while (true)
                                if (executor.Disposed)
                                    goto before_executor;
                                else if (reader != null)
                                    yield return default;
                                else if (routine.MoveNext())
                                    yield return routine.Current;
                                else
                                    break;
                        }
                    }
            }
            Dispose();
        }

        public bool OnReader(in CodeReader reader, out ExecutionStatus output)
        {
            this.reader = reader;
            bool moveNext = OnTick(out output);
            this.reader = null;
            return moveNext;
        }

        public bool OnTick(out ExecutionStatus output)
        {
            if (!Disposed)
            {
                int loops = 0;
            again:
                if (routine.MoveNext())
                {
                    if (routine.Current.code == CMD_STATUS.RETURN)
                        if (++loops < 100)
                            goto again;

                    output = routine.Current;
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
            executors.Clear();
            vstack.Clear();
        }
    }
}