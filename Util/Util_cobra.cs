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
            char_CHAIN = '&',
            char_PIPE = '|',
            char_BACKPIPE = '!';

        //--------------------------------------------------------------------------------------------------------------

        public static char GetRotator(in float speed = 10) => ((int)(Time.unscaledTime * speed) % 4) switch
        {
            0 => '|',
            1 => '/',
            2 => '-',
            3 => '\\',
            _ => '?',
        };

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
                    while (read_i > 0 && text[read_i] == c)
                        --read_i;
                    return Conclude(ref read_i);

                case char_SPACE:
                case char_TAB:
                    --read_i;
                    while (read_i > 0 && text[read_i - 1] switch
                    {
                        char_SPACE => true,
                        char_TAB => true,
                        _ => false,
                    })
                        --read_i;
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
                        char_SPACE or char_TAB => false,
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

        public static bool TryReadArgument(this string text, out int start_i, ref int read_i, out string argument)
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

                    case char_SPACE:
                    case char_TAB:
                    case char_NEWLINE:
                    case char_CHAIN:
                    case char_PIPE:
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

        public static int SkipCharactersUntil(this string text, ref int read_i, params char[] key_chars)
        {
            HashSet<char> charSet = new(key_chars);
            int skips = 0;

            while (read_i >= 0 && read_i < text.Length)
            {
                char c = text[read_i];

                if (skips == 0)
                    switch (c)
                    {
                        case '"':
                        case '\'':
                            ++read_i;
                            while (read_i < text.Length && text[read_i] != c)
                            {
                                if (text[read_i] == '\\')
                                    ++read_i;
                                ++read_i;
                                ++skips;
                            }
                            if (read_i < text.Length)
                                ++read_i;
                            return skips;
                    }

                if (c == char_NEWLINE)
                    if (!charSet.Contains(char_NEWLINE))
                        return skips;

                if (charSet.Contains(c))
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

        public static int SkipCharactersUntil_inverted(this string text, ref int read_i, in bool positive, params char[] key_chars)
        {
            HashSet<char> charSet = new(key_chars);
            int skips = 0;

            while (read_i > 0 && read_i <= text.Length)
            {
                if (read_i > 0)
                    --read_i;

                char c = text[read_i];

                if (c == char_NEWLINE)
                    if (!charSet.Contains(char_NEWLINE))
                    {
                        ++read_i;
                        return skips;
                    }

                if (positive == charSet.Contains(c))
                {
                    ++read_i;
                    return skips;
                }

                if (read_i < text.Length)
                    --read_i;

                ++skips;
            }
            return skips;
        }

        public static bool TryReadPipe(this string text, ref int read_i)
        {
            SkipCharactersUntil(text, ref read_i, char_CHAIN, char_PIPE);
            if (read_i < text.Length && text[read_i] == char_PIPE)
            {
                ++read_i;
                return true;
            }
            else
                return false;
        }

        public static bool TryReadChain(this string text, ref int read_i)
        {
            SkipCharactersUntil(text, ref read_i, char_CHAIN, char_PIPE);
            if (read_i + 1 < text.Length && text[read_i] == char_CHAIN && text[read_i + 1] == char_CHAIN)
            {
                ++read_i;
                ++read_i;
                return true;
            }
            else
                return false;
        }
    }
}