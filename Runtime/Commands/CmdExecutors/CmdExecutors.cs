namespace _COBRA_
{
    internal static partial class CmdExecutors
    {
        static Command domain_exe;

        //--------------------------------------------------------------------------------------------------------------

        public static void Init()
        {
            domain_exe = Shell.static_domain.AddDomain("executor");
            Init_Find();
            Init_Monitor();
        }
    }
}