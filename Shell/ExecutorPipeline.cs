using System;
using System.Collections.Generic;

namespace _COBRA_
{
    internal class ExecutorPipeline : IDisposable
    {
        public readonly List<Command.Executor> executors = new();

        //--------------------------------------------------------------------------------------------------------------

        public bool TryGetCurrent(out Command.Executor executor)
        {
            for (int i = 0; i < executors.Count; i++)
                if (!executors[i].disposed.Value)
                {
                    executor = executors[i];
                    return true;
                }
            executor = null;
            return false;
        }

        //--------------------------------------------------------------------------------------------------------------

        public bool AreAllDisposed()
        {
            for (int i = 0; i < executors.Count; i++)
                if (!executors[i].disposed.Value)
                    return false;
            return true;
        }

        //--------------------------------------------------------------------------------------------------------------

        public void Dispose()
        {
            for (int i = 0; i < executors.Count; i++)
                executors[i].PropagateDispose();
            executors.Clear();
        }
    }
}