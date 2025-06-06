namespace _COBRA_
{
    partial class Command
    {
        partial class Line
        {
            public bool TryReadInt(out int value)
            {
                if (TryReadArgument(out string arg, out _, lint: false))
                    if (int.TryParse(arg, out value))
                    {
                        LintToThisPosition(linter.literal);
                        return true;
                    }
                    else
                        ReadBack();
                value = 0;
                return false;
            }

            public bool TryReadFloat(out float value)
            {
                if (TryReadArgument(out string arg, out _, lint: false))
                    if (arg.TryParseFloat(out value))
                    {
                        LintToThisPosition(linter.literal);
                        return true;
                    }
                    else
                        ReadBack();
                value = 0;
                return false;
            }
        }
    }
}