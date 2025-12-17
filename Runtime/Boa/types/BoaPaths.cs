namespace _COBRA_.Boa
{
    public readonly struct BoaPath
    {
        public readonly string _value;
        public override readonly string ToString() => _value;
        public BoaPath(in string path) => _value = path;
        public static implicit operator BoaPath(in string path) => new(path);
        public static implicit operator string(in BoaPath path) => path._value;
    }

    public readonly struct BoaFPath
    {
        public readonly string _value;
        public override readonly string ToString() => _value;
        public BoaFPath(in string path) => _value = path;
        public static implicit operator BoaFPath(in string path) => new(path);
        public static implicit operator string(in BoaFPath path) => path._value;
    }

    public readonly struct BoaDPath
    {
        public readonly string _path;
        public override readonly string ToString() => _path;
        public BoaDPath(in string path) => _path = path;
        public static implicit operator BoaDPath(in string path) => new(path);
        public static implicit operator string(in BoaDPath path) => path._path;
    }
}