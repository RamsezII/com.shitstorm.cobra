using _UTIL_;
using System;
using System.Collections.Generic;

namespace _COBRA_.Boa.execution
{
    public sealed class Executor : Disposable
    {
        public readonly Action onDone;
        public readonly Action action;
        public readonly IEnumerator<ExecutionOutput> routine;

        //----------------------------------------------------------------------------------------------------------

        public Executor(in Action onDone, in Action action, in IEnumerator<ExecutionOutput> routine)
        {
            this.onDone = onDone;
            this.action = action;
            this.routine = routine;
        }
    }
}