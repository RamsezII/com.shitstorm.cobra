using System;

partial class Util_cobra
{
    public static bool ToBool(this object s) => s is bool b && b || s.ToString().Equals("true", StringComparison.OrdinalIgnoreCase);
}