using System;

partial class Util_cobra
{
    public static bool ToBool(this object data) => data switch
    {
        null => false,
        bool b => b,
        int i => i > 0,
        float f => f > 0,
        _ => data.ToString().Equals("true", StringComparison.OrdinalIgnoreCase),
    };
}