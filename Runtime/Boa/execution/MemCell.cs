using System;

namespace _COBRA_.Boa
{
    [Serializable]
    public struct MemCell
    {
        public readonly Type type;
        public object value;

        //----------------------------------------------------------------------------------------------------------

        public MemCell(in Type type, in object value)
        {
            this.type = type;
            this.value = value;
        }

        public MemCell(in Type type) : this(type, null)
        {
        }

        public MemCell(in object value) : this(value.GetType(), null)
        {
        }
    }
}