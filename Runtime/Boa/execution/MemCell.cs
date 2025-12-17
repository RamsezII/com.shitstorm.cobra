using System;
using System.Collections.Generic;
using UnityEngine;

namespace _COBRA_.Boa
{
    [Serializable]
    public struct MemCell
    {
        public object _value;
        public readonly Type _type;
        public readonly T As<T>() => (T)_value;
        public override readonly string ToString() => $"cell[{_value}]";
        public override readonly bool Equals(object obj) => obj is MemCell cell && EqualityComparer<object>.Default.Equals(_value, cell._value);
        public override readonly int GetHashCode() => HashCode.Combine(_value);

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

        //----------------------------------------------------------------------------------------------------------

        public static bool operator true(MemCell a) => (bool)a._value;
        public static bool operator false(MemCell a) => !(bool)a._value;

        public static implicit operator bool(MemCell a) => (bool)a._value;
        public static implicit operator MemCell(bool a) => new(a);

        public static implicit operator int(MemCell a) => a._value is int i ? i : Mathf.RoundToInt((float)a._value);
        public static implicit operator MemCell(int a) => new(a);

        public static implicit operator float(MemCell a) => a._value is int i ? i : (float)a._value;
        public static implicit operator MemCell(float a) => new(a);

        public static MemCell operator +(MemCell a) => a._value is int i ? i : (float)a._value;
        public static MemCell operator -(MemCell a) => a._value is int i ? -i : -(float)a._value;
        public static MemCell operator !(MemCell a) => !(bool)a._value;

        public static MemCell operator ==(MemCell a, MemCell b) => a._value.Equals(b._value);
        public static MemCell operator !=(MemCell a, MemCell b) => !a._value.Equals(b._value);

        public static MemCell operator +(MemCell a, MemCell b) => a._value is int ia && b._value is int ib ? ia + ib : (float)a._value + (float)b._value;
        public static MemCell operator -(MemCell a, MemCell b) => a._value is int ia && b._value is int ib ? ia - ib : (float)a._value - (float)b._value;
        public static MemCell operator *(MemCell a, MemCell b) => a._value is int ia && b._value is int ib ? ia * ib : (float)a._value * (float)b._value;
        public static MemCell operator /(MemCell a, MemCell b) => a._value is int ia && b._value is int ib ? ia / ib : (float)a._value / (float)b._value;
        public static MemCell operator %(MemCell a, MemCell b) => a._value is int ia && b._value is int ib ? ia % ib : (float)a._value % (float)b._value;

        public static MemCell operator &(MemCell a, MemCell b) => a._value is bool ba && b._value is bool bb ? ba && bb : (int)a._value & (int)b._value;
        public static MemCell operator |(MemCell a, MemCell b) => a._value is bool ba && b._value is bool bb ? ba || bb : (int)a._value | (int)b._value;
        public static MemCell operator ^(MemCell a, MemCell b) => a._value is bool ba && b._value is bool bb ? ba ^ bb : (int)a._value ^ (int)b._value;

        public static MemCell operator ++(MemCell a) => a._value is int i ? 1 + i : 1 + (float)a._value;
        public static MemCell operator --(MemCell a) => a._value is int i ? 1 - i : 1 - (float)a._value;
    }
}