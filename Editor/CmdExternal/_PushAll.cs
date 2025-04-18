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

            Command.static_domain.AddRoutine("push-all-repos",
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
                routine: EPush);

            static IEnumerator<CMD_STATUS> EPush(Command.Executor exe)
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

            Command.static_domain.AddRoutine("pull-all-repos",
                opts: static exe =>
                {
                    if (exe.line.TryRead_one_of_the_flags(exe, out string flag, flag_blocking))
                        exe.opts.Add(flag_blocking, null);
                },
                routine: EPull);

            static IEnumerator<CMD_STATUS> EPull(Command.Executor exe)
            {
                bool blocking = exe.opts.ContainsKey(flag_blocking);

                if (blocking)
                    GitBatchPusher.PullAllGitRepos();
                else
                {
                    var routine = GitBatchPusher.EPullAllGitRepos();
                    while (routine.MoveNext())
                        yield return new CMD_STATUS(CMD_STATES.BLOCKING, progress: routine.Current);
                }
            }
        }
    }
}