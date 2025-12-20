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

        internal protected virtual void OnExecutorsQueue(in Queue<Executor> executors)
        {
        }
    }
}