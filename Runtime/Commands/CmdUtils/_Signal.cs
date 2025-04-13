namespace _COBRA_
{
    partial class CmdUtils
    {
        static void Init_Signal()
        {
            Shell.static_domain.AddAction(
                "signal",
                manual: new("sending custom signals to executors"),
                opts: static exe =>
                {

                },
                action: static exe =>
                {

                });
        }
    }
}