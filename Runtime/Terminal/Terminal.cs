namespace _COBRA_
{
    public interface ITerminal
    {
        public Command.Line.Linter GetLinter { get; }
        public Shell GetShell { get; }
        void ToggleWindow(bool toggle);
        void ForceSelectStdin();
        void ForceStdin(in string text);
    }
}