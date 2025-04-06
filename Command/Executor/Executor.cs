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
            //prefixe = $"{MachineSettings.machine_name.Value.SetColor("#73CC26")}:{NUCLEOR.terminal_path.SetColor("#73B2D9")}$",

            static readonly Executor echo_executor = new(new() { new("echo", new("echo", on_data: (exe, stdin) => Debug.Log(stdin))), }, Line.EMPTY_EXE);

            public readonly string cmd_name;
            public readonly Command command;
            public readonly string cmd_path;

            public Line line;
            readonly Executor stdout_exe = echo_executor, stderr_exe = echo_executor;
            public readonly List<object> args;
            public IEnumerator<CMD_STATUS> routine;

            public readonly ThreadSafe_struct<bool> disposed = new();

            public int executions = -1;
            static byte id_counter;
            public byte id = ++id_counter;

            public string error;
            public void Stdout(in object data) => stdout_exe.command.on_data(stdout_exe, data);
            void Stderr(in object data) => stderr_exe.command.on_data(stderr_exe, data);

            //--------------------------------------------------------------------------------------------------------------

            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
            static void OnBeforeSceneLoad()
            {
                id_counter = 0;
            }

            //--------------------------------------------------------------------------------------------------------------

            public Executor(in List<KeyValuePair<string, Command>> path, in Line line)
            {
                this.line = line;
                cmd_name = path[^1].Key;
                command = path[^1].Value;

                switch (path.Count)
                {
                    case 0:
                        throw new ArgumentException($"Command path is empty.", nameof(path));

                    case 1 when path[0].Value == cmd_root_shell:
                        cmd_path = "~";
                        break;

                    case 1:
                        cmd_path = path[0].Key;
                        break;

                    default:
                        StringBuilder path_sb = new();
                        for (int i = 0; i < path.Count; i++)
                            path_sb.Append(path[i].Key + "/");
                        path_sb.Remove(path_sb.Length - 1, 1);
                        cmd_path = path_sb.ToString();
                        break;
                }

                if (command.args != null)
                {
                    args = new(command.init_min_args_required);
                    command.args(this);

                    if (error == null)
                        if (args.Count < command.init_min_args_required)
                            error = $"[{nameof(command.init_min_args_required)}] '{cmd_name}' ({cmd_path}) requires {command.init_min_args_required} arguments to init, {args.Count} were given.";
                }

                if (error == null)
                    if (line.TryReadPipe())
                        if (cmd_root_shell.TryReadCommand_path(line, out var path2))
                        {
                            Executor executor = new(path2, line);
                            error = executor.error;
                            if (error == null)
                                if (executor.command.on_data == null)
                                    error = $"Command '{executor.cmd_name}' ({executor.cmd_path}) has no '{nameof(executor.command.on_data)}' callback, it can not be piped into.";
                                else
                                    stdout_exe = executor;
                        }
                        else if (line.signal == CMD_SIGNALS.EXEC && line.start_i != line.cpl_start_i)
                            error = $"Command '{cmd_name}' ({cmd_path}) failed to parse pipe.";

                if (error != null)
                    if (CanLogError())
                        Debug.LogWarning($"[ERROR] '{cmd_name}' ({cmd_path}): {error}");
            }

            //--------------------------------------------------------------------------------------------------------------

            public void Dispose()
            {
                lock (disposed)
                {
                    if (disposed._value)
                    {
                        Debug.LogWarning($"[{nameof(Dispose)}] '{cmd_name}' ({cmd_path}) is already disposed.");
                        return;
                    }
                    disposed._value = true;

                    OnDispose();
                }
            }

            protected virtual void OnDispose()
            {
            }
        }
    }
}