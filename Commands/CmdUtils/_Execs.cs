using System.Collections.Generic;

namespace _COBRA_
{
    partial class CmdUtils
    {
        static void Init_Execs()
        {
            Shell.static_domain.AddAction(
                "current-executors",
                args: exe =>
                {

                },
                action: exe =>
                {
                    List<string> lines = new();

                    for (int i = 0; i < exe.shell.active_exe_pipelines_stack.Count; ++i)
                    {
                        ExecutorPipeline stack = exe.shell.active_exe_pipelines_stack[i];
                        for (int j = 0; j < stack.executors.Count; ++j)
                        {
                            Command.Executor exe2 = stack.executors[j];
                            if (exe2 != null)
                            {
                                lines.Add($" {exe2.id,-3} | {"front",-20} | '{exe2.command.name}' ({exe2.cmd_path})");
                            }
                        }
                    }

                    for (int i = 0; i < exe.shell.background_executors.Count; ++i)
                    {
                        Command.Executor exe2 = exe.shell.background_executors[i];
                        if (exe2 != null)
                        {
                            lines.Add($" {exe2.id,-3} | {"background",-20} | '{exe2.command.name}' ({exe2.cmd_path})");
                        }
                    }

                    foreach (var e in exe.shell.pending_executors_queue)
                    {
                        if (e != null)
                        {
                            lines.Add($" {e.id,-3} | {"pending",-20} | '{e.command.name}' ({e.cmd_path})");
                        }
                    }

                    exe.Stdout(lines);
                },
                aliases: "ps");
        }
    }
}