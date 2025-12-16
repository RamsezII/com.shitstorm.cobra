using _COBRA_.Boa;
using System;
using System.Collections.Generic;

namespace _COBRA_
{
    internal sealed class Executor
    {
        internal readonly Action<Janitor> action_SIG_EXE;
        internal readonly Func<Janitor, IEnumerator<ExecutionOutput>> routine_SIG_EXE, routine_SIG_READER;

        //----------------------------------------------------------------------------------------------------------

        public Executor(
            in Action<Janitor> action_SIG_EXE = null,
            in Func<Janitor, IEnumerator<ExecutionOutput>> routine_SIG_EXE = null,
            in Func<Janitor, IEnumerator<ExecutionOutput>> routine_SIG_READER = null
        )
        {
            this.action_SIG_EXE = action_SIG_EXE;
            this.routine_SIG_EXE = routine_SIG_EXE;
            this.routine_SIG_READER = routine_SIG_READER;
        }
    }
}