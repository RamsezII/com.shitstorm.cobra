using _ARK_;
using _UTIL_;

namespace _COBRA_
{
    partial class Shell
    {
        public readonly ValueHandler<string> workdir = new(ArkPaths.instance.Value.dpath_home);

        public ExecutionStatus RegularStatus() => new(
            code: CMD_STATUS.WAIT_FOR_STDIN,
            prefixe: new(
                text: $"{ArkMachine.user_name.Value}:{workdir._value}$ ",
                lint: $"{ArkMachine.user_name.Value.SetColor("#73CC26")}:{workdir._value.SetColor("#73B2D9")}$ "
                )
        );

        //--------------------------------------------------------------------------------------------------------------

        internal void ChangeWorkdir(in string path) => workdir.Value = Util_cobra.PathCheck(workdir._value, path, PathModes.ForceFull, false, false, out _, out _);
    }
}