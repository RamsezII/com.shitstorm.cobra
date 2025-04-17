using _COBRA_;
using _UTIL_e;
using System.Collections.Generic;

namespace _COBRA_e
{
    partial class CmdExternal
    {
        static void Init_PushAll()
        {
            const string
                flag_blocking = "--blocking";

            domain_git.AddRoutine("push-all-repos",
                manual: new("<commit message>"),
                min_args: 1,
                opts: static exe =>
                {
                    if (exe.line.TryRead_one_of_the_flags(exe, out string flag, flag_blocking))
                        exe.opts.Add(flag_blocking, null);
                },
                args: static exe =>
                {
                    if (exe.line.TryReadArgument(out string arg, out _))
                        exe.args.Add(arg);
                },
                routine: ERoutine);

            static IEnumerator<CMD_STATUS> ERoutine(Command.Executor exe)
            {
                string arg = (string)exe.args[0];
                bool blocking = exe.opts.ContainsKey(flag_blocking);

                if (blocking)
                    GitBatchPusher.PushAllGitRepos(arg);
                else
                {
                    var routine = GitBatchPusher.EPushAllGitRepos(arg);
                    while (routine.MoveNext())
                        yield return new CMD_STATUS(CMD_STATES.BLOCKING, progress: routine.Current);
                }
            }
        }
    }
}