using _UTIL_;
using System.Collections.Generic;

namespace _COBRA_.Boa
{
    public sealed class Janitor : Disposable
    {
        public readonly Shell shell;
        public readonly MScope<MemCell> mem_scope = new(parent: null);
        public readonly List<MemCell> mem_stack = new();
        internal readonly Queue<Executor> exe_stack = new();

        internal CodeReader reader;

        readonly IEnumerator<ExecutionOutput> routine;

        //----------------------------------------------------------------------------------------------------------

        internal Janitor(in Shell shell, in BoaProgram program)
        {
            this.shell = shell;
            for (int i = program.asts.Count - 1; i >= 0; i--)
                program.asts[i].OnExecutionStack(this);
            routine = ERoutine();
        }

        //----------------------------------------------------------------------------------------------------------

        IEnumerator<ExecutionOutput> ERoutine()
        {
            while (exe_stack.TryDequeue(out var exe_cell))
            {
                if (exe_cell.routine_SIG_READER != null)
                {
                    while (reader == null)
                        yield return default;

                    using var routine = exe_cell.routine_SIG_READER(this);

                    while (true)
                        if (reader == null)
                            yield return default;
                        else if (routine.MoveNext())
                            yield return routine.Current;
                        else
                            break;
                }

                if (exe_cell.action_SIG_EXE != null)
                {
                    while (reader != null)
                        yield return default;
                    exe_cell.action_SIG_EXE(this);
                }

                if (exe_cell.routine_SIG_EXE != null)
                {
                    while (reader != null)
                        yield return default;

                    using var routine = exe_cell.routine_SIG_EXE(this);

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
                if (routine.MoveNext())
                {
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
            exe_stack.Clear();
        }
    }
}