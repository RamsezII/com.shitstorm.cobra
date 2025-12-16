using System;

namespace _COBRA_.Boa.execution
{
    [Serializable]
    public struct MemCell
    {
        public readonly Type type;
        public object value;
    }

    public sealed class VScope : MScope<MemCell>
    {

        //----------------------------------------------------------------------------------------------------------

        internal VScope(in VScope parent) : base(parent)
        {
        }
    }
}