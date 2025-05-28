using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;

namespace _COBRA_
{
    internal static class CmdExternal
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnAfterSceneLoad()
        {
            const string
                flag_read_all = "--read-all",
                flag_r = "-r";

            Command.static_domain.AddAction(
                "os-terminal",
                opts: static exe => exe.signal.TryReadOption_workdir(exe),
                action: static exe =>
                {
                    string workdir = exe.GetWorkdir();
                    ProcessStartInfo psi = new()
                    {
                        FileName = GetTerminalCommand(),
                        WorkingDirectory = workdir,
                        UseShellExecute = true,
                    };
                    Process.Start(psi);

                    static string GetTerminalCommand()
                    {
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                            return "powershell.exe";
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                            return "gnome-terminal";
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                            return "open -a Terminal";

                        throw new PlatformNotSupportedException("Unsupported OS platform.");
                    }
                });

            Command.static_domain.AddAction(
                "run-external-command",
                min_args: 1,
                opts: static exe =>
                {
                    if (exe.signal.TryRead_one_of_the_flags(exe, out string flag, flag_read_all, flag_r))
                        exe.opts.Add(flag_r, null);
                    exe.signal.TryReadOption_workdir(exe);
                },
                args: static exe =>
                {
                    if (exe.opts.ContainsKey(flag_r))
                    {
                        if (exe.signal.TryReadArguments(out string command_line))
                            exe.args.Add(command_line);
                    }
                    else
                    {
                        if (exe.signal.TryReadArgument(out string command_line, out _, lint: false))
                        {
                            exe.signal.LintToThisPosition(exe.signal.linter.external);
                            exe.args.Add(command_line);
                        }
                    }
                },
                action: static exe =>
                {
                    string command_line = (string)exe.args[0];
                    string workdir = exe.GetWorkdir();
                    Util.RunExternalCommand(workdir, command_line, on_stdout: stdout => exe.Stdout(stdout));
                },
                aliases: "."
            );
        }
    }
}