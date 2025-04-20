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

            public Line line;
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

            internal Executor(in Shell shell, in Executor parent, in Line line, in List<Command> path, in bool parse_options = true, in bool parse_arguments = true)
            {
                instances.Add(this);

                this.shell = shell;
                this.parent = parent;
                this.path = path;
                command = path[^1];

                if (line.signal.HasFlag(SIGNALS.EXEC))
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
                            ParseOptions(line);
                    }

                if (error == null)
                    if (command.args != null)
                    {
                        args = new(command.min_args);
                        if (parse_arguments)
                            ParseArguments(line);
                    }

                before_parsing_separator:
                if (error == null)
                    if (line.TryReadCommandSeparator(out string spr))
                    {
                        bool
                            is_pipe = spr.Equals("|", StringComparison.OrdinalIgnoreCase),
                            is_chain = spr.Equals("&&", StringComparison.OrdinalIgnoreCase),
                            is_background = spr.Equals("&", StringComparison.OrdinalIgnoreCase);

                        if (!is_pipe && !is_chain && !is_background)
                            error = $"'{line.arg_last}' is no valid command separator";

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
                                if (static_domain.TryReadCommand_path(line, out var path2, pipe_only: is_pipe))
                                {
                                    Executor exe = new(shell, this, line, path2);
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
                                    error = $"{this} failed to pipe into unknown command ({nameof(line.arg_last)}: '{line.arg_last}')";
                                else if (is_chain)
                                    error = $"{this} failed to chain into unknown command ({nameof(line.arg_last)}: '{line.arg_last}')";
                    }

                if (error == null)
                    if (background)
                        PropagateBackground();

                if (error == null)
                    if (command.routine != null)
                        if (line.signal.HasFlag(SIGNALS.EXEC))
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
                if (opts.TryGetValue_str(Line.opt_workdir, out string workdir))
                    return shell.PathCheck(workdir, PathModes.ForceFull);
                return shell.working_dir;
            }

            public void Stdout(in object data, string lint = null)
            {
                if (string.IsNullOrEmpty(lint))
                    lint = data.ToString();

                if (stdout_exe == null)
                    if (line != null && line.shell != null && line.shell.terminal != null)
                        line.shell.terminal.AddLine(data, lint);
                    else
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
                else
                {
                    stdout_exe.line = line;
                    stdout_exe.janitor = janitor;
                    stdout_exe.command.on_pipe(stdout_exe, data);
                    stdout_exe.line = null;
                }
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

                line = null;
            }
        }
    }
}