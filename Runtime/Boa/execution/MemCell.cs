using System;

namespace _COBRA_.Boa
{
    [Serializable]
    public struct MemCell
    {
        public readonly Type type;
        public dynamic value;
        public override readonly string ToString() => $"cell[{value}({type})]";

        //----------------------------------------------------------------------------------------------------------

        public MemCell(in Type type, in dynamic value)
        {
            this.type = type;
            this.value = value;
        }

        public MemCell(in object value) : this(value.GetType(), value)
        {
        }
    }
}