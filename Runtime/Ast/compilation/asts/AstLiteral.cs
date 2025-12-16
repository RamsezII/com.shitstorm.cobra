namespace _COBRA_.Boa.compilation
{
    internal class AstLiteral<T> : AstExpression
    {
        public T value;
        public AstLiteral(in T value) : base(typeof(T))
        {
            this.value = value;
        }
    }
}