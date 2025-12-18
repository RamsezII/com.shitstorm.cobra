using System;

namespace _COBRA_.Boa
{
    internal class AstLiteral : AstExpression
    {
        public object value;

        //----------------------------------------------------------------------------------------------------------

        public AstLiteral(in object value, in Type output_type = null) : base(output_type ?? value.GetType())
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
                    janitor.vstack.Add(item: new MemCell(value));
                }
            ));
        }
    }
}