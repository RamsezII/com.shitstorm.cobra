using _ARK_;
using _UTIL_;

namespace _COBRA_
{
    partial class Shell
    {
        public readonly ValueNotifier<string> workdir = new(NUCLEOR.DFHome.FullName);

        public ExecutionStatus RegularStatus() => new(
            code: CMD_STATUS.WAIT_FOR_STDIN,
            prefixe: new(
                text: $"{NUCLEOR.user_name._value}:{workdir._value}$ ",
                lint: $"{NUCLEOR.user_name._value.SetColor("#73CC26")}:{workdir._value.SetColor("#73B2D9")}$ "
                )
        );

        //--------------------------------------------------------------------------------------------------------------

        internal void ChangeWorkdir(in string path) => workdir.Value = Util_cobra.PathCheck(workdir._value, path, PathModes.ForceFull, false, false, out _, out _);

        public string PathCheck(in string path, in PathModes mode) => Util_cobra.PathCheck(workdir._value, path, mode, true, false, out _, out _);
    }
}