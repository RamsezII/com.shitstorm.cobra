namespace _COBRA_
{
    public struct CobraNumber
    {
        public float _value;

        //----------------------------------------------------------------------------------------------------------

        public CobraNumber(float value)
        {
            _value = value;
        }

        //----------------------------------------------------------------------------------------------------------

        public static implicit operator float(CobraNumber n) => n._value;
        public static implicit operator CobraNumber(float n) => new(n);

        public static explicit operator int(CobraNumber n) => (int)n._value;
        public static implicit operator CobraNumber(int n) => new(n);

        public static CobraNumber operator +(CobraNumber n) => n._value;
        public static CobraNumber operator -(CobraNumber n) => -n._value;

        public static CobraNumber operator +(CobraNumber a, CobraNumber b) => new(a._value + b._value);
        public static CobraNumber operator -(CobraNumber a, CobraNumber b) => new(a._value - b._value);
        public static CobraNumber operator *(CobraNumber a, CobraNumber b) => new(a._value * b._value);
        public static CobraNumber operator /(CobraNumber a, CobraNumber b) => new(a._value / b._value);
        public static CobraNumber operator %(CobraNumber a, CobraNumber b) => new(a._value % b._value);

        public static CobraNumber operator ++(CobraNumber a) => new(1 + a._value);
        public static CobraNumber operator --(CobraNumber a) => new(1 - a._value);
    }
}