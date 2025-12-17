using System.Collections.Generic;

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

        protected override void OnExecutionQueue(in Janitor janitor, in List<Executor> executors)
        {
            base.OnExecutionQueue(janitor, executors);

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