using _UTIL_;
using System.Collections.Generic;

namespace _COBRA_.Boa
{
    public sealed class Janitor : Disposable
    {
        public readonly Shell shell;
        readonly Queue<AstAbstract> asts;

        public readonly VScope vscope;
        public readonly List<MemCell> vstack = new();
        internal readonly Queue<Executor> executors = new();

        internal CodeReader reader;

        readonly IEnumerator<ExecutionOutput> routine;

        //----------------------------------------------------------------------------------------------------------

        internal Janitor(in Shell shell, in VScope vscope, in Queue<AstAbstract> asts)
        {
            this.shell = shell;
            this.vscope = vscope;
            this.asts = asts;
            routine = ERoutine();
        }

        //----------------------------------------------------------------------------------------------------------

        IEnumerator<ExecutionOutput> ERoutine()
        {
            while (asts.TryDequeue(out var ast))
            {
                ast.OnExecutionStack(this);
                while (executors.TryDequeue(out var executor))
                {
                    if (executor.routine_SIG_READER != null)
                    {
                        while (reader == null)
                            yield return default;

                        using var routine = executor.routine_SIG_READER(this);

                        while (true)
                            if (reader == null)
                                yield return default;
                            else if (routine.MoveNext())
                                yield return routine.Current;
                            else
                                break;
                    }

                    if (executor.action_SIG_EXE != null)
                    {
                        while (reader != null)
                            yield return default;
                        executor.action_SIG_EXE(this);
                    }

                    if (executor.routine_SIG_EXE != null)
                    {
                        while (reader != null)
                            yield return default;

                        using var routine = executor.routine_SIG_EXE(this);

                        while (true)
                            if (reader != null)
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

        public bool OnReader(in CodeReader reader, out ExecutionOutput output)
        {
            this.reader = reader;
            bool moveNext = OnTick(out output);
            this.reader = null;
            return moveNext;
        }

        public bool OnTick(out ExecutionOutput output)
        {
            if (!Disposed)
            {
                int loops = 0;
            again:
                if (routine.MoveNext())
                {
                    if (routine.Current.status == CMD_STATUS.RETURN)
                        if (++loops < 100)
                            goto again;

                    output = routine.Current;
                    return true;
                }
                Dispose();
            }
            output = default;
            return false;
        }

        //----------------------------------------------------------------------------------------------------------

        protected override void OnDispose()
        {
            base.OnDispose();
            routine?.Dispose();
            executors.Clear();
            vscope._vars.Clear();
            vstack.Clear();
        }
    }
}