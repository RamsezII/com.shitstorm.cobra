using System;

namespace _COBRA_.Boa
{
    [Serializable]
    public struct MemCell
    {
        public readonly Type type;
        public object value;
    }
}