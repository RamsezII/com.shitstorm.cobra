partial class Util_cobra
{
    public static bool TryReadArguments(this string text, out int start_i, ref int read_i, out string arguments)
    {
        start_i = read_i;
        if (string.IsNullOrWhiteSpace(text))
        {
            arguments = string.Empty;
            return false;
        }

        SkipSpaces(text, ref read_i);

        start_i = read_i;
        while (read_i < text.Length)
        {
            char c = text[read_i];
            switch (c)
            {
                case '\\':
                    ++read_i;
                    break;

                case '"':
                case '\'':
                    ++read_i;
                    while (read_i < text.Length && text[read_i] != c)
                    {
                        if (text[read_i] == '\\')
                            ++read_i;
                        ++read_i;
                    }
                    ++read_i;
                    break;
            }
            ++read_i;
        }

        return TryRead(start_i, ref read_i, out arguments);

        bool TryRead(in int start_i, ref int read_i, out string argument)
        {
            if (read_i > text.Length)
                read_i = text.Length;

            if (read_i > start_i)
            {
                if (read_i < text.Length)
                    argument = text[start_i..read_i];
                else
                    argument = text[start_i..];

                if (argument.Length >= 2 && argument[0] == argument[^1])
                    switch (argument[0])
                    {
                        case '"':
                        case '\'':
                            argument = argument[1..^1];
                            break;
                    }

                return true;
            }
            argument = string.Empty;
            return false;
        }
    }

    public static bool TryReadArgument(this string text, out int start_i, ref int read_i, out string argument, in bool stop_at_separators)
    {
        start_i = read_i;
        if (string.IsNullOrWhiteSpace(text))
        {
            argument = string.Empty;
            return false;
        }

        SkipSpaces(text, ref read_i);

        start_i = read_i;
        while (read_i < text.Length)
        {
            char c = text[read_i];
            switch (c)
            {
                case '\\':
                    ++read_i;
                    break;

                case '&' or '|' when stop_at_separators:
                case ' ' or '\t' or '\n':
                    return TryRead(start_i, ref read_i, out argument);

                case '"':
                case '\'':
                    ++read_i;
                    while (read_i < text.Length && text[read_i] != c)
                    {
                        if (text[read_i] == '\\')
                            ++read_i;
                        ++read_i;
                    }
                    ++read_i;
                    return TryRead(start_i, ref read_i, out argument);
            }
            ++read_i;
        }

        return TryRead(start_i, ref read_i, out argument);

        bool TryRead(in int start_i, ref int read_i, out string argument)
        {
            if (read_i > text.Length)
                read_i = text.Length;

            if (read_i > start_i)
            {
                if (read_i < text.Length)
                    argument = text[start_i..read_i];
                else
                    argument = text[start_i..];

                if (argument.Length >= 2 && argument[0] == argument[^1])
                    switch (argument[0])
                    {
                        case '"':
                        case '\'':
                            argument = argument[1..^1];
                            break;
                    }

                return true;
            }
            argument = string.Empty;
            return false;
        }
    }
}