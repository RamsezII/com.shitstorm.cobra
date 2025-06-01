partial class Util_cobra
{
    public static int GroupedErase(this string text, ref int read_i)
    {
        int start_i = read_i;

    reset:
        if (read_i <= 0)
            return Conclude(ref read_i);

        char c = text[read_i - 1];
        switch (c)
        {
            case '\n':
                --read_i;
                while (read_i > 0 && text[read_i - 1] == c)
                    --read_i;
                if (read_i > 0)
                    if (start_i - read_i > 1)
                        ++read_i;
                return Conclude(ref read_i);

            case ' ':
            case '\t':
                --read_i;
                while (read_i > 0 && text[read_i - 1] == c)
                    --read_i;
                if (read_i > 0 && text[read_i - 1] == '\n')
                    return Conclude(ref read_i);
                goto reset;

            case '"':
            case '\'':
                --read_i;
                while (read_i > 0)
                {
                    if (read_i > 2 && text[read_i - 2] == '\\')
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
                    ' ' or '\t' or '\n' => false,
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
}