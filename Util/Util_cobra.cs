using System.Collections.Generic;
using UnityEngine;

namespace _COBRA_
{
    public static class Util_cobra
    {
        public const char
            CHAR_SPACE = ' ',
            CHAR_TAB = '\t',
            CHAR_NEWLINE = '\n',
            CHAR_CHAIN = '&',
            CHAR_PIPE = '|',
            CHAR_BACKPIPE = '!';

        //--------------------------------------------------------------------------------------------------------------

        public static char GetRotator(in float speed = 10) => ((int)(Time.unscaledTime * speed) % 4) switch
        {
            0 => '|',
            1 => '/',
            2 => '-',
            3 => '\\',
            _ => '?',
        };

        static void Increment(ref int read_i, in bool left_to_right)
        {
            if (left_to_right)
                ++read_i;
            else
                --read_i;
        }

        public static int SkipCharactersUntil(this string text, ref int read_i, in bool left_to_right, in bool positive, params char[] key_chars)
        {
            HashSet<char> charSet = new(key_chars);
            int skips = 0;

            while (left_to_right
                ? read_i >= 0 && read_i < text.Length
                : read_i > 0 && read_i <= text.Length)
            {
                if (!left_to_right)
                    Increment(ref read_i, left_to_right);

                char c = text[read_i];

                if (c == CHAR_NEWLINE)
                    if (!charSet.Contains(CHAR_NEWLINE))
                    {
                        if (!left_to_right)
                            ++read_i;
                        return skips;
                    }

                if (positive == charSet.Contains(c))
                {
                    if (!left_to_right)
                        ++read_i;
                    return skips;
                }

                switch (c)
                {
                    case '"':
                    case '\'':
                        Increment(ref read_i, left_to_right);
                        SkipCharactersUntil(text, ref read_i, true, true, c);
                        break;

                    case '\\':
                        Increment(ref read_i, left_to_right);
                        break;
                }

                if (left_to_right)
                    Increment(ref read_i, left_to_right);

                ++skips;
            }
            return skips;
        }

        public static bool TryReadPipe(this string text, ref int read_i)
        {
            SkipCharactersUntil(text, ref read_i, true, true, CHAR_CHAIN, CHAR_PIPE);
            if (read_i < text.Length && text[read_i] == CHAR_PIPE)
            {
                ++read_i;
                return true;
            }
            else
                return false;
        }

        public static bool TryReadChain(this string text, ref int read_i)
        {
            SkipCharactersUntil(text, ref read_i, true, true, CHAR_CHAIN, CHAR_PIPE);
            if (read_i + 1 < text.Length && text[read_i] == CHAR_CHAIN && text[read_i + 1] == CHAR_CHAIN)
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