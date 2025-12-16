namespace _COBRA_.Boa
{
    internal class AstLiteral<T> : AstExpression
    {
        public T value;

        //----------------------------------------------------------------------------------------------------------

        public AstLiteral(in T value) : base(typeof(T))
        {
            this.value = value;
        }

        //----------------------------------------------------------------------------------------------------------

        internal override void OnExecutionStack(Janitor janitor)
        {
            base.OnExecutionStack(janitor);

            janitor.executors.Enqueue(new(
                action_SIG_EXE: janitor =>
                {
                    janitor.vstack.Add(new(typeof(T), value));
                }
            ));
        }
    }
}