namespace _COBRA_
{
    public interface ITerminal
    {
        public Command.Signal.Linter GetLinter { get; }
        public Shell GetShell { get; }
        void ToggleWindow(bool toggle);
        void ForceSelectStdin();
        void SetStdin(string text);
        void AddLine(in object line, in string lint);
        void ClearLines();
        void Exit();
    }
}