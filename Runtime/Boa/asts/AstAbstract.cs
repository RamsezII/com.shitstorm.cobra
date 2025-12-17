using System.Collections.Generic;

namespace _COBRA_.Boa
{
    internal abstract class AstAbstract
    {
        //----------------------------------------------------------------------------------------------------------

        internal AstAbstract()
        {
        }

        //----------------------------------------------------------------------------------------------------------

        internal void EnqueueExecutors(in Janitor janitor) => EnqueueExecutors(janitor, out _);
        internal void EnqueueExecutors(in Janitor janitor, out List<Executor> executors)
        {
            executors = new();
            OnExecutionQueue(janitor, executors);
            if (executors.Count > 0)
                for (int i = 0; i < executors.Count; i++)
                    janitor.executors.Enqueue(executors[i]);
        }

        protected virtual void OnExecutionQueue(in Janitor janitor, in List<Executor> executors)
        {
        }
    }
}