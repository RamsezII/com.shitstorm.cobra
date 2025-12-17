using System;

namespace _COBRA_.Boa
{
    public readonly struct BoaNumber
    {
        public readonly dynamic value;
        public readonly Type type;

        //----------------------------------------------------------------------------------------------------------

        public BoaNumber(in int value)
        {
            this.value = value;
            type = typeof(int);
        }

        public BoaNumber(in float value)
        {
            this.value = value;
            type = typeof(float);
        }

        //----------------------------------------------------------------------------------------------------------

        public static implicit operator BoaNumber(int n) => new(n);
        public static implicit operator BoaNumber(float n) => new(n);
        public static explicit operator int(BoaNumber n) => n.value;
        public static implicit operator float(BoaNumber n) => n.value;

        public static BoaNumber operator +(BoaNumber a, BoaNumber b) => a.value + b.value;
        public static BoaNumber operator -(BoaNumber a, BoaNumber b) => a.value - b.value;
        public static BoaNumber operator *(BoaNumber a, BoaNumber b) => a.value * b.value;
        public static BoaNumber operator /(BoaNumber a, BoaNumber b) => a.value / b.value;
        public static BoaNumber operator %(BoaNumber a, BoaNumber b) => a.value % b.value;
    }
}