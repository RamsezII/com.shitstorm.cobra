using System;

public static partial class Util_cobra
{
    public static Type EnglobingType(in Type a, in Type b)
    {
        if (a == null || b == null)
            return null;

        if (a.IsAssignableFrom(b))
            return a;
        if (b.IsAssignableFrom(a))
            return b;

        return typeof(object);
    }

    public static int SkipSpaces(this string text, ref int read_i)
    {
        int start_i = read_i;
        while (read_i < text.Length && text[read_i] switch
        {
            ' ' => true,
            '\t' => true,
            _ => false,
        })
            ++read_i;
        return read_i - start_i;
    }

    public static bool HasNext(this string text, ref int read_i, in bool ignore_case = true, in string skippables = " \t\n\r")
    {
        var ordinal = ignore_case.ToOrdinal();
        while (read_i < text.Length)
            if (skippables != null && skippables.Contains(text[read_i], ordinal))
                ++read_i;
            else
                return read_i < text.Length;
        return false;
    }

    public static bool TryReadChar(this string text, ref int read_i, out char ch)
    {
        if (!string.IsNullOrWhiteSpace(text))
            for (; read_i < text.Length; ++read_i)
                switch (text[read_i])
                {
                    case ' ':
                        break;

                    default:
                        ch = text[read_i];
                        return true;
                }
        ch = '\0';
        return false;
    }
}