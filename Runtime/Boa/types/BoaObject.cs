using System;
using UnityEngine;

namespace _COBRA_
{
    public struct BoaObject
    {
        public object _value;
        public readonly Type _type;
        public readonly BoaList AsList => _value as BoaList;
        public readonly T As<T>() => (T)_value;
        public override readonly bool Equals(object obj) => base.Equals(obj);
        public override readonly int GetHashCode() => HashCode.Combine(_value);

        //----------------------------------------------------------------------------------------------------------

        public BoaObject(in Type type, in object value)
        {
            if (type == typeof(BoaObject) || value is BoaObject)
                throw new ArgumentException($"boa object does not accept boa object");
            _type = type;
            _value = value is BoaObject boa ? boa._value : value;
        }

        public BoaObject(in object value) : this((value is BoaObject boa ? boa : value).GetType(), value)
        {
        }

        //----------------------------------------------------------------------------------------------------------

        public static bool operator true(BoaObject a) => (bool)a._value;
        public static bool operator false(BoaObject a) => !(bool)a._value;

        public static implicit operator bool(BoaObject a) => (bool)a._value;
        public static implicit operator BoaObject(bool a) => new(a);

        public static implicit operator int(BoaObject a) => a._value is int i ? i : Mathf.RoundToInt((float)a._value);
        public static implicit operator BoaObject(int a) => new(a);

        public static implicit operator float(BoaObject a) => a._value is int i ? i : (float)a._value;
        public static implicit operator BoaObject(float a) => new(a);

        public static BoaObject operator +(BoaObject a) => a._value is int i ? i : (float)a._value;
        public static BoaObject operator -(BoaObject a) => a._value is int i ? -i : -(float)a._value;
        public static BoaObject operator !(BoaObject a) => !(bool)a._value;

        public static BoaObject operator ==(BoaObject a, BoaObject b) => a._value.Equals(b._value);
        public static BoaObject operator !=(BoaObject a, BoaObject b) => !a._value.Equals(b._value);

        public static BoaObject operator +(BoaObject a, BoaObject b) => a._value is int ia && b._value is int ib ? ia + ib : (float)a._value + (float)b._value;
        public static BoaObject operator -(BoaObject a, BoaObject b) => a._value is int ia && b._value is int ib ? ia - ib : (float)a._value - (float)b._value;
        public static BoaObject operator *(BoaObject a, BoaObject b) => a._value is int ia && b._value is int ib ? ia * ib : (float)a._value * (float)b._value;
        public static BoaObject operator /(BoaObject a, BoaObject b) => a._value is int ia && b._value is int ib ? ia / ib : (float)a._value / (float)b._value;
        public static BoaObject operator %(BoaObject a, BoaObject b) => a._value is int ia && b._value is int ib ? ia % ib : (float)a._value % (float)b._value;

        public static BoaObject operator &(BoaObject a, BoaObject b) => a._value is bool ba && b._value is bool bb ? ba && bb : (int)a._value & (int)b._value;
        public static BoaObject operator |(BoaObject a, BoaObject b) => a._value is bool ba && b._value is bool bb ? ba || bb : (int)a._value | (int)b._value;
        public static BoaObject operator ^(BoaObject a, BoaObject b) => a._value is bool ba && b._value is bool bb ? ba ^ bb : (int)a._value ^ (int)b._value;

        public static BoaObject operator ++(BoaObject a) => a._value is int i ? 1 + i : 1 + (float)a._value;
        public static BoaObject operator --(BoaObject a) => a._value is int i ? 1 - i : 1 - (float)a._value;
    }
}