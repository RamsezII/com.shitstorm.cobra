namespace _COBRA_
{
    public readonly struct CobraPath
    {
        public readonly string _value;
        public override readonly string ToString() => _value;
        public CobraPath(in string path) => _value = path;
        public static implicit operator CobraPath(in string path) => new(path);
        public static implicit operator string(in CobraPath path) => path._value;
    }

    public readonly struct CobraFPath
    {
        public readonly string _value;
        public override readonly string ToString() => _value;
        public CobraFPath(in string path) => _value = path;
        public static implicit operator CobraFPath(in string path) => new(path);
        public static implicit operator string(in CobraFPath path) => path._value;
    }

    public readonly struct CobraDPath
    {
        public readonly string _path;
        public override readonly string ToString() => _path;
        public CobraDPath(in string path) => _path = path;
        public static implicit operator CobraDPath(in string path) => new(path);
        public static implicit operator string(in CobraDPath path) => path._path;
    }
}