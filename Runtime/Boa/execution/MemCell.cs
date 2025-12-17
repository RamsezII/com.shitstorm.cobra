using System;

namespace _COBRA_.Boa
{
    [Serializable]
    public class MemCell
    {
        public readonly Type type;
        public dynamic value;
        public override string ToString() => $"cell[{value}({type})]";

        //----------------------------------------------------------------------------------------------------------

        public MemCell(in Type type = null, in object value = null)
        {
            this.type = type ?? value.GetType();
            this.value = value;
        }
    }
}