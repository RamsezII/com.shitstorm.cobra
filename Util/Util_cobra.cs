using System.Collections.Generic;
using UnityEngine;

namespace _COBRA_
{
    public static class Util_cobra
    {
        public const char
            char_SPACE = ' ',
            char_DQUOTE = '"',
            char_SQUOTE = '\'',
            char_SLASH = '/',
            char_BACKSLASH = '\\',
            char_TAB = '\t',
            char_NEWLINE = '\n',
            char_BACKGROUND = '&',
            char_PIPE = '|';

        public const string
            str_CHAIN = "&&",
            str_PIPE = "|";

        public static readonly string[]
            str_OPERATORS = new string[]
            {
                "==", "!=", "<=", ">=", "<", ">", "=", "+", "-", "*", "/", "%", "&", "|", "^", "~",
                "<<", ">>", "&&", "||", "??"
            },
            str_CMD_SEPARATORS = new string[]
            {
                "&&", "|", "&",
            };

        //--------------------------------------------------------------------------------------------------------------

        public static int SkipSpaces(this string text, ref int read_i)
        {
            int start_i = read_i;
            while (read_i < text.Length && text[read_i] switch
            {
                char_SPACE => true,
                char_TAB => true,
                _ => false,
            })
                ++read_i;
            return read_i - start_i;
        }

        public static bool HasNext(this string text, ref int read_i)
        {
            if (!string.IsNullOrWhiteSpace(text))
                if (read_i < text.Length)
                    while (read_i < text.Length)
                        if (text[read_i] switch
                        {
                            char_SPACE or char_TAB or char_NEWLINE => true,
                            _ => false,
                        })
                            ++read_i;
                        else
                            return read_i < text.Length;
            return false;
        }

        public static bool TryReadPipe(this string text, ref int read_i)
        {
            if (text.HasNext(ref read_i))
                return text[read_i] == char_PIPE;
            return false;
        }

        public static int GroupedErase(this string text, ref int read_i)
        {
            int start_i = read_i;

        reset:
            if (read_i <= 0)
                return Conclude(ref read_i);

            char c = text[read_i - 1];
            switch (c)
            {
                case char_NEWLINE:
                    --read_i;
                    while (read_i > 0 && text[read_i - 1] == c)
                        --read_i;
                    if (read_i > 0)
                        if (start_i - read_i > 1)
                            ++read_i;
                    return Conclude(ref read_i);

                case char_SPACE:
                case char_TAB:
                    --read_i;
                    while (read_i > 0 && text[read_i - 1] == c)
                        --read_i;
                    if (read_i > 0 && text[read_i - 1] == char_NEWLINE)
                        return Conclude(ref read_i);
                    goto reset;

                case char_SQUOTE:
                case char_DQUOTE:
                    --read_i;
                    while (read_i > 0)
                    {
                        if (read_i > 2 && text[read_i - 2] == char_BACKSLASH)
                            --read_i;
                        else if (text[read_i - 1] == c)
                        {
                            --read_i;
                            break;
                        }
                        --read_i;
                    }
                    return Conclude(ref read_i);

                default:
                    while (read_i > 0 && text[read_i - 1] switch
                    {
                        char_SPACE or char_TAB or char_NEWLINE => false,
                        _ => true,
                    })
                        --read_i;
                    return Conclude(ref read_i);
            }

            int Conclude(ref int read_i)
            {
                if (read_i < 0)
                    read_i = 0;
                return start_i - read_i;
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
                            case char_SQUOTE:
                            case char_DQUOTE:
                                argument = argument[1..^1];
                                break;
                        }

                    return true;
                }
                argument = string.Empty;
                return false;
            }

            start_i = read_i;
            while (read_i < text.Length)
            {
                char c = text[read_i];
                switch (c)
                {
                    case char_BACKSLASH:
                        ++read_i;
                        break;

                    case char_BACKGROUND or char_PIPE when stop_at_separators:
                    case char_SPACE or char_TAB or char_NEWLINE:
                        return TryRead(start_i, ref read_i, out argument);

                    case '"':
                    case '\'':
                        ++read_i;
                        while (read_i < text.Length && text[read_i] != c)
                        {
                            if (text[read_i] == char_BACKSLASH)
                                ++read_i;
                            ++read_i;
                        }
                        ++read_i;
                        return TryRead(start_i, ref read_i, out argument);
                }
                ++read_i;
            }

            return TryRead(start_i, ref read_i, out argument);
        }

        public static int SkipCharactersUntilNo(this string text, ref int read_i, params char[] key_chars)
        {
            HashSet<char> charSet = new(key_chars);
            int skips = 0;

            while (read_i >= 0 && read_i < text.Length)
            {
                char c = text[read_i];

                if (c == char_NEWLINE)
                    if (!charSet.Contains(char_NEWLINE))
                        return skips;

                if (!charSet.Contains(c))
                    return skips;

                switch (c)
                {
                    case '"':
                    case '\'':
                        --read_i;
                        return skips;

                    case '\\':
                        ++read_i;
                        break;
                }

                if (read_i < text.Length)
                    ++read_i;

                ++skips;
            }
            return skips;
        }
    }
}