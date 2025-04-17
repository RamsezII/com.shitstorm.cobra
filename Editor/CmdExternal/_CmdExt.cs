using System;
using System.Collections.Generic;
using System.IO;
using _COBRA_;

namespace _COBRA_e
{
    partial class CmdExternal
    {
        /*
            git
            ├── init
            ├── clone [url]
            ├── status
            ├── add [file...]
            ├── commit [-m "message"]
            ├── checkout [branch]
            ├── push
            ├── pull
            ├── merge [branch]
            └── branch
                ├── -d [branch]
                └── [new-branch]
        */

        static void Init_CmdExt()
        {
            Command.static_domain.AddAction(
                "run-external-command",
                min_args: 1,
                opts: static exe =>
                {
                    Dictionary<string, Action<string>> opts = new(StringComparer.OrdinalIgnoreCase)
                    {
                        {
                            opt_workdir,
                            opt =>
                            {
                                if (exe.line.TryReadArgument(out string cmd_dir, out _, path_mode: PATH_FLAGS.DIRECTORY))
                                {
                                    cmd_dir = exe.shell.PathCheck(cmd_dir, PathModes.ForceFull);
                                    if (Directory.Exists(cmd_dir))
                                        exe.opts.Add(opt_workdir, cmd_dir);
                                    else
                                        exe.error = $"could not find directory at path: '{cmd_dir}'";
                                }
                                else
                                    exe.error = $"please specify valid directory path for option '{opt_workdir}'";
                            }
                        }
                    };
                    exe.line.TryRead_options_parsed(exe, opts);
                },
                args: static exe =>
                {
                    if (exe.line.TryReadArgument(out string arg0, out bool is_candidate, new[] { "git", }))
                    {
                        exe.args.Add(arg0);
                        // if (is_candidate)

                    }
                },
                action: static exe =>
                {
                    string command_line = (string)exe.args[0];
                    string workdir = exe.shell.working_dir;
                    if (exe.opts.TryGetValue(opt_workdir, out object _val))
                        workdir = (string)_val;
                    Util_e.RunExternalCommand(workdir, command_line);
                },
                aliases: "."
            );
        }
    }
}