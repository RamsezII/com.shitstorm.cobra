using System.Collections.Generic;

namespace _COBRA_.Boa
{
    public abstract class AstAbstract
    {
        //----------------------------------------------------------------------------------------------------------

        internal AstAbstract()
        {
        }

        //----------------------------------------------------------------------------------------------------------

        internal protected virtual void OnExecutorsQueue(MemStack memstack, MemScope memscope, in Queue<Executor> executors)
        {
        }
    }
}