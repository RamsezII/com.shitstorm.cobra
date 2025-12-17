using System;

namespace _COBRA_.Boa
{
    [Serializable]
    public class MemCell
    {
        public object _value;
        public readonly Type _type;
        public BoaObject AsBoa => new(_value);
        public T As<T>() => (T)_value;
        public override string ToString() => $"cell[{_value}]";

        //----------------------------------------------------------------------------------------------------------

        public MemCell(in object value)
        {
            _value = value;
            _type = value.GetType();
        }

        public MemCell(in Type type, in object value)
        {
            _type = type;
            _value = value;
        }
    }
}