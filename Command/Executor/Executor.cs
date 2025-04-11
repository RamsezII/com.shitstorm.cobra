using _ARK_;
using _UTIL_;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace _COBRA_
{
    partial class Command
    {
        public partial class Executor : IDisposable
        {
            internal static readonly Executor exe_log = new(
                shell: null,
                line: new Line(string.Empty, SIGNAL_FLAGS._none_, null),
                path: new()
                {
                    new Command("_log", on_pipe: (exe, args, data) =>
                    {
                        switch (data)
                        {
                            case IEnumerable<string> lines:
                                foreach (string line in lines)
                                    Debug.Log(line);
                                break;

                            case string str:
                                Debug.Log(str);
                                break;

                            default:
                                Debug.Log(data);
                                break;
                        }
                    }),
                }
                );

            public readonly Shell shell;
            public readonly Command command;
            public readonly string cmd_path;

            public Line line;
            internal ExecutorPipeline pipeline;
            public bool background;
            public Executor stdout_exe = exe_log;
            public readonly List<object> args;
            public readonly Dictionary<string, object> opts;
            public IEnumerator<CMD_STATUS> routine;

            public readonly ThreadSafe_struct<bool> disposed = new();

            public int executions = -1;
            static ushort PID_counter;
            public ushort PID = ++PID_counter;

            public string error;

            //--------------------------------------------------------------------------------------------------------------

            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
            static void OnBeforeSceneLoad()
            {
                PID_counter = 0;
            }

            //--------------------------------------------------------------------------------------------------------------

            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
            static void OnAfterSceneLoad()
            {
                Init_Execs();
            }

            //--------------------------------------------------------------------------------------------------------------

            internal Executor(in Shell shell, in Line line, in List<Command> path, in bool parse_options = true, in bool parse_arguments = true)
            {
                this.shell = shell;
                command = path[^1];

                switch (path.Count)
                {
                    case 0:
                        throw new ArgumentException($"Command path is empty.", nameof(path));

                    case 1 when path[0] == Shell.static_domain:
                        cmd_path = "~";
                        break;

                    case 1:
                        cmd_path = path[0].name;
                        break;

                    default:
                        StringBuilder path_sb = new();
                        for (int i = 0; i < path.Count; i++)
                            path_sb.Append(path[i].name + "/");
                        path_sb.Remove(path_sb.Length - 1, 1);
                        cmd_path = path_sb.ToString();
                        break;
                }

                if (error == null)
                    if (command.opts != null)
                    {
                        opts = new(StringComparer.OrdinalIgnoreCase);
                        if (parse_options)
                            ParseOptions(line);
                    }

                if (error == null)
                    if (command.args != null)
                    {
                        args = new(command.min_args);
                        if (parse_arguments)
                            ParseArguments(line);
                    }

                if (error == null)
                    if (line.TryReadPipe())
                        if (Shell.static_domain.TryReadCommand_path(line, out var path2, pipe_only: true))
                        {
                            Executor exe = new(shell, line, path2);
                            if (exe.error != null)
                                error = $"'{command.name}' ({cmd_path}) -> {exe.error}";
                            else if (exe.command.on_pipe == null)
                                error = $"'{command.name}' ({cmd_path}) -> '{exe.command.name}' ({exe.cmd_path}) has no '{nameof(exe.command.on_pipe)}' callback, it can not be piped into.";
                            else
                                stdout_exe = exe;
                        }
                        else
                            error = $"'{command.name}' ({cmd_path}) failed to pipe into unknown command '{line.arg_last}'";

                if (error == null)
                    if (command.routine != null)
                        if (line.signal.HasFlag(SIGNAL_FLAGS.EXEC))
                            routine = command.routine(this);
            }

            internal void ParseOptions(in Line line)
            {
                if (command.opts == null)
                    return;

                this.line = line;
                command.opts(this);
                this.line = null;
            }

            internal void ParseArguments(in Line line)
            {
                if (command.args == null)
                    return;

                this.line = line;
                command.args(this);
                this.line = null;

                if (error == null)
                    if (args.Count < command.min_args || args.Count > command.max_args)
                        if (command.min_args == command.max_args)
                            error = $"'{command.name}' ({cmd_path}) expects {command.min_args} arguments, {args.Count} were given.";
                        else
                            error = $"'{command.name}' ({cmd_path}) accepts from {command.min_args} to {command.max_args} arguments, {args.Count} were given.";
            }

            //--------------------------------------------------------------------------------------------------------------

            public void PropagateBackground()
            {
                background = true;
                if (stdout_exe != exe_log)
                    stdout_exe.background = true;
            }

            public static string GetPrefixe() => $"{MachineSettings.machine_name.Value.SetColor("#73CC26")}:{"~".SetColor("#73B2D9")}$";
            public string GetPrefixe(string user_name = null, string cmd_path = null)
            {
                user_name ??= MachineSettings.machine_name.Value;
                cmd_path ??= this.cmd_path;
                return $"{user_name.SetColor("#73CC26")}:{cmd_path.SetColor("#73B2D9")}$";
            }

            public void Stdout(in object data)
            {
                stdout_exe.line = line;
                if (stdout_exe != exe_log)
                    stdout_exe.pipeline = pipeline;
                stdout_exe.command.on_pipe(stdout_exe, stdout_exe.args, data);
                stdout_exe.line = null;
            }

            //--------------------------------------------------------------------------------------------------------------

            public void PropagateDispose()
            {
                if (!disposed.Value)
                    if (stdout_exe != exe_log)
                        stdout_exe.Dispose();
                Dispose();
            }

            public void Dispose()
            {
                lock (disposed)
                {
#if UNITY_EDITOR
                    if (shell != null)
                        shell.Janitize($"[{PID}] {cmd_path} (already janitized: {disposed._value})");
#endif

                    if (disposed._value)
                        return;
                    disposed.Value = true;
                }

                routine?.Dispose();
                routine = null;

                if (args != null)
                {
                    for (int i = 0; i < args.Count; ++i)
                        if (args[i] is IDisposable disposable)
                            disposable.Dispose();
                    args.Clear();
                }

                line = null;
            }
        }
    }
}