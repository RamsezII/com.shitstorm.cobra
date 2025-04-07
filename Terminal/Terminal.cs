namespace _COBRA_
{
    public interface ITerminal
    {
        public Command.Line.Linter Linter { get; }
        public Command.Executor RootExecutor { get; }
        void ToggleWindow(bool toggle);
    }
}