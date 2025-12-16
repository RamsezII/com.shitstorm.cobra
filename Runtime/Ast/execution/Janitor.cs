using _UTIL_;
using _COBRA_.Boa.compilation;
using System.Collections.Generic;

namespace _COBRA_.Boa.execution
{
    public sealed class Janitor : Disposable
    {
        public readonly Shell shell;
        public readonly VScope vscope = new(parent: null);
        public readonly VStack vstack = new();
        internal readonly List<Executor> exe_stack = new();

        internal CodeReader reader;

        readonly IEnumerator<ExecutionOutput> routine;

        //----------------------------------------------------------------------------------------------------------

        internal Janitor(in Shell shell, in AstProgram program)
        {
            this.shell = shell;
            for (int i = program.asts.Count - 1; i >= 0; i--)
                program.asts[i].OnExecutionStack(this);
            routine = ERoutine();
        }

        //----------------------------------------------------------------------------------------------------------

        IEnumerator<ExecutionOutput> ERoutine()
        {
            while (exe_stack.Count > 0)
            {
                using var exe = exe_stack[^1];
                exe_stack.RemoveAt(exe_stack.Count - 1);

                exe.action?.Invoke();

                if (exe.routine != null)
                    while (!exe.Disposed && exe.routine.MoveNext())
                        yield return exe.routine.Current;

                exe.onDone?.Invoke();
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

            routine.Dispose();

            for (int i = 0; i < exe_stack.Count; i++)
                exe_stack[i].Dispose();
            exe_stack.Clear();
        }
    }
}