using _COBRA_;

namespace _COBRA_
{
    public enum PATH_ENUMS : byte
    {
        File,
        Dir,
    }

    public enum PATH_FLAGS : byte
    {
        _none_,
        FILE = 1 << PATH_ENUMS.File,
        DIRECTORY = 1 << PATH_ENUMS.Dir,
        BOTH = FILE | DIRECTORY,
    }
}

partial class Util_cobra
{
    public static bool HasFlags_any(this PATH_FLAGS mask, PATH_FLAGS flags) => (mask & flags) != 0;
}