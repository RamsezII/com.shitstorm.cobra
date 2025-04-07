namespace _COBRA_
{
    public interface ITerminal
    {
        public static ITerminal instance;

        //----------------------------------------------------------------------------------------------------------

        protected virtual void Awake()
        {
            instance = this;
        }

        //----------------------------------------------------------------------------------------------------------
        
        public void Toggle(in bool toggle)
        {

        }

        //----------------------------------------------------------------------------------------------------------

        protected virtual void OnDestroy()
        {
            if (this == instance)
                instance = null;
        }
    }
}