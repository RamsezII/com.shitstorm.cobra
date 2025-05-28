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
            public readonly Shell shell;
            public readonly Executor parent;
            public readonly Command command;
            public readonly string cmd_longname;
            public readonly List<Command> path;

            public Signal signal;
            internal Janitor janitor;
            public bool background;
            internal Executor stdout_exe, next_exe;
            public readonly List<object> args;
            public readonly VarDict opts;
            public IEnumerator<CMD_STATUS> routine;

            internal bool started, disposed;

            public int executions = -1;
            static ushort PID_counter;
            public readonly ushort EID;
            public ushort PEID => parent?.EID ?? 0;

            public string error;
            public override string ToString() => $"[{parent?.EID ?? 0}-{EID} {cmd_longname}]";

            //--------------------------------------------------------------------------------------------------------------

            internal Executor(in Shell shell, in Executor parent, in Signal signal, in List<Command> path, in bool parse_options = true, in bool parse_arguments = true)
            {
                instances.Add(this);

                this.shell = shell;
                this.parent = parent;
                this.path = path;
                command = path[^1];

                if (signal.flags.HasFlag(SIG_FLAGS.EXEC))
                    EID = ++PID_counter;

                switch (path.Count)
                {
                    case 0:
                        throw new ArgumentException($"Command path is empty.", nameof(path));

                    case 1 when path[0] == static_domain:
                        cmd_longname = "~";
                        break;

                    case 1:
                        cmd_longname = path[0].name;
                        break;

                    default:
                        StringBuilder path_sb = new();
                        for (int i = 0; i < path.Count; i++)
                            path_sb.Append(path[i].name + "/");
                        path_sb.Remove(path_sb.Length - 1, 1);
                        cmd_longname = path_sb.ToString();
                        break;
                }

                if (error == null)
                    if (command.opts != null)
                    {
                        opts = new();
                        if (parse_options)
                            ParseOptions(signal);
                    }

                if (error == null)
                    if (command.args != null)
                    {
                        args = new(command.min_args);
                        if (parse_arguments)
                            ParseArguments(signal);
                    }

                before_parsing_separator:
                if (error == null)
                    if (signal.TryReadCommandSeparator(out string spr))
                    {
                        bool
                            is_pipe = spr.Equals("|", StringComparison.OrdinalIgnoreCase),
                            is_chain = spr.Equals("&&", StringComparison.OrdinalIgnoreCase),
                            is_background = spr.Equals("&", StringComparison.OrdinalIgnoreCase);

                        if (!is_pipe && !is_chain && !is_background)
                            error = $"'{signal.arg_last}' is no valid command separator";

                        if (error == null)
                            if (is_background)
                                if (background)
                                    error = $"already informed background status.";
                                else
                                {
                                    background = true;
                                    goto before_parsing_separator;
                                }

                        if (error == null)
                            if (is_pipe || is_chain)
                                if (static_domain.TryReadCommand_path(signal, out var path2, pipe_only: is_pipe))
                                {
                                    Executor exe = new(shell, this, signal, path2);
                                    if (exe.error != null)
                                        error = this + exe.error;
                                    else if (is_pipe && exe.command.on_pipe == null)
                                        error = $"{this} -> {exe} has no '{nameof(exe.command.on_pipe)}' callback, it can not be piped into.";
                                    else if (is_pipe)
                                        if (command.output_type.IsAssignableFrom(exe.command.input_type))
                                            stdout_exe = exe;
                                        else
                                            error = $"{this}.{nameof(command.output_type)} can not be piped into {exe}.{nameof(exe.command.input_type)} {exe.command.input_type}";
                                    else
                                        next_exe = exe;
                                }
                                else if (is_pipe)
                                    error = $"{this} failed to pipe into unknown command ({nameof(signal.arg_last)}: '{signal.arg_last}')";
                                else if (is_chain)
                                    error = $"{this} failed to chain into unknown command ({nameof(signal.arg_last)}: '{signal.arg_last}')";
                    }

                if (error == null)
                    if (background)
                        PropagateBackground();

                if (error == null)
                    if (command.routine != null)
                        if (signal.HasFlags_any(SIG_FLAGS.EXEC | SIG_FLAGS.TICK))
                            routine = command.routine(this);
            }

            internal void ParseOptions(in Signal signal)
            {
                if (command.opts == null)
                    return;

                this.signal = signal;
                command.opts(this);
                this.signal = null;
            }

            internal void ParseArguments(in Signal signal)
            {
                if (command.args == null)
                    return;

                this.signal = signal;
                command.args(this);
                this.signal = null;

                if (error == null)
                    if (args.Count < command.min_args || args.Count > command.max_args)
                        if (command.min_args == command.max_args)
                            error = $"{this} expects {command.min_args} arguments, {args.Count} were given.";
                        else
                            error = $"{this} accepts from {command.min_args} to {command.max_args} arguments, {args.Count} were given.";
            }

            //--------------------------------------------------------------------------------------------------------------

            internal void LogBackgroundStart() => Debug.Log($"{this} started running in background".ToSubLog());

            internal void PropagateBackground()
            {
                background = true;
                if (stdout_exe != null)
                    stdout_exe.background = true;
                next_exe?.PropagateBackground();
            }

            internal bool TryPullNext(out Executor exe)
            {
                if (next_exe == null)
                {
                    exe = null;
                    return false;
                }
                exe = next_exe;
                next_exe = null;
                return true;
            }

            public string GetWorkdir()
            {
                if (opts.TryGetValue_str(Signal.opt_workdir, out string workdir))
                    return shell.PathCheck(workdir, PathModes.ForceFull);
                return shell.working_dir;
            }

            public void Stdout(in object data, string lint = null, Signal signal = null)
            {
                signal ??= this.signal;

                if (data == null)
                    lint = null;

                if (stdout_exe != null)
                {
                    stdout_exe.signal = signal;
                    stdout_exe.janitor = janitor;
                    stdout_exe.command.on_pipe(stdout_exe, data);
                    stdout_exe.signal = null;
                    error = stdout_exe.error;
                }
                else if (data != null)
                    if (signal != null && signal.shell != null && signal.shell.terminal != null)
                        if (lint == null)
                            foreach (string str in data.IterateThroughData_str())
                                signal.shell.terminal.AddLine(str, str);
                        else
                            signal.shell.terminal.AddLine(data, lint ?? data.ToString());
                    else
                        foreach (object o in data.IterateThroughData())
                            Debug.Log(o);
            }

            //--------------------------------------------------------------------------------------------------------------

            internal void Janitize()
            {
                if (!disposed)
                    stdout_exe?.Dispose();
                Dispose();
            }

            public void Dispose()
            {
                instances.Remove(this);

                if (disposed)
                    return;
                disposed = true;

                routine?.Dispose();
                routine = null;

                if (args != null)
                {
                    for (int i = 0; i < args.Count; ++i)
                        if (args[i] is IDisposable disposable)
                            disposable.Dispose();
                    args.Clear();
                }

                signal = null;
            }
        }
    }
}