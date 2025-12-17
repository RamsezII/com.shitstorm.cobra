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

        protected internal override void OnExecutorsQueue(in Janitor janitor)
        {
            base.OnExecutorsQueue(janitor);

            janitor.executors.Enqueue(new(
                name: $"literal({value})",
                action_SIG_EXE: janitor =>
                {
                    janitor.vstack.Add(new(type: typeof(T), value: value));
                }
            ));
        }
    }
}