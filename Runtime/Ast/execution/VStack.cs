using System.Collections.Generic;

namespace _COBRA_.Boa.execution
{
    public sealed class VStack
    {
        internal readonly List<object> _stack = new();

        //----------------------------------------------------------------------------------------------------------

        public object Pop()
        {
            object value = _stack[^1];
            _stack.RemoveAt(_stack.Count - 1);
            return value;
        }
    }
}