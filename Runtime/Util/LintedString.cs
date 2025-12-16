namespace _COBRA_
{
    public readonly struct LintedString
    {
        public static readonly LintedString EMPTY = new(string.Empty, string.Empty);
        public readonly string text, lint;

        public LintedString(in string text, in Colors color) : this(text, text.SetColor(color))
        {
        }

        public LintedString(in string text, in string lint = null)
        {
            this.text = text;
            this.lint = lint ?? text;
        }
    }
}