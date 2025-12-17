namespace _COBRA_
{
    public readonly struct LintedString
    {
        readonly string text, lint;
        public readonly string Text => text ?? string.Empty;
        public readonly string Lint => lint ?? string.Empty;

        //----------------------------------------------------------------------------------------------------------

        public LintedString(in string text, in string lint = null)
        {
            this.text = text ?? string.Empty;
            this.lint = lint ?? text;
        }

        public LintedString(in string text, in Colors color) : this(text, text.SetColor(color))
        {
        }
    }
}