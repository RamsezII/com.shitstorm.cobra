namespace _COBRA_
{
    internal static partial class CmdExecutors
    {
        static Command domain_exe;

        //--------------------------------------------------------------------------------------------------------------

        public static void Init()
        {
            domain_exe = Command.static_domain.AddDomain("executor");

            Init_Find();
            Init_Monitor();

            domain_exe.AddAction(
                "list-all",
                manual: new("returns all active executors"),
                action: static exe =>
                {
                    exe.Stdout(Shell.Monitor());
                });
        }
    }
}