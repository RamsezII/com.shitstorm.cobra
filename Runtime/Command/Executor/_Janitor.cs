using System;
using System.Collections.Generic;
using UnityEngine;

namespace _COBRA_
{
    partial class Command
    {
        partial class Executor
        {
            internal class Janitor : IDisposable
            {
                internal readonly List<Executor> _executors = new();
                bool disposed;

                static ushort id_counter;
                public readonly ushort pipeline_ID = ++id_counter;

                internal string error;

                public readonly VarDict temp_vars = new();

                //--------------------------------------------------------------------------------------------------------------

                internal Janitor(in Line line, in Executor exe)
                {
                    AddExecutor(line, exe);
                }

                //--------------------------------------------------------------------------------------------------------------

                internal bool TryPullError(out string error)
                {
                    error = this.error;
                    this.error = null;
                    return error != null;
                }

                internal string PullError()
                {
                    string err = error;
                    error = null;
                    return err;
                }

                internal void AddExecutor(in Line line, in Executor exe)
                {
                    if (disposed)
                        Debug.LogError($"adding {exe.GetType().FullName} '{exe.command.name}' ({exe.cmd_longname}) to disposed pipeline[{pipeline_ID}].");

                    exe.janitor = this;

                    if (_executors.Remove(exe))
                        Debug.LogWarning($"'{exe.GetType().FullName}' '{exe.command.name}' ({exe.cmd_longname}) already exists in pipeline[{pipeline_ID}]. Replacing it.");

                    _executors.Add(exe);
                    TryExecute(line, exe);
                }

                internal bool TryGetCurrent(out Executor executor)
                {
                    for (int i = _executors.Count - 1; i >= 0; i--)
                    {
                        executor = _executors[i];
                        if (!executor.disposed)
                            return true;
                        else if (executor.TryPullNext(out executor))
                        {
                            _executors.Add(executor);
                            return true;
                        }
                    }
                    executor = null;
                    return false;
                }

                internal bool TryExecuteCurrent(in Line line, out Executor exe)
                {
                before_execution:
                    exe = null;
                    for (int i = 0; i <= _executors.Count; i++)
                    {
                        if (i == _executors.Count)
                        {
                            exe = null;
                            return false;
                        }

                        exe = _executors[i];

                        if (!exe.disposed)
                            break;
                        else if (exe.TryPullNext(out exe))
                        {
                            if (exe.background)
                                Shell.background_janitors.Add(new Janitor(line, exe));
                            else
                                _executors.Add(exe);
                            return true;
                        }
                    }

                    if (!TryExecute(line, exe))
                        return false;

                    if (exe.disposed)
                        goto before_execution;

                    return true;
                }

                bool TryExecute(in Line line, in Executor exe)
                {
                    if (exe.command.action == null && exe.routine == null)
                    {
                        Debug.LogError($"'{exe.GetType().FullName}' '{exe.command.name}' ({exe.cmd_longname}) has no {nameof(exe.command.action)} or {nameof(exe.routine)} to execute.");
                        return false;
                    }

                    exe.line = line;

                    if (exe.command.action != null)
                        if (line.HasFlags_any(SIGNALS.EXEC | SIGNALS.TICK))
                        {
                            if (exe.started)
                                Debug.LogWarning($"'{exe.GetType().FullName}' '{exe.command.name}' ({exe.cmd_longname}) has already been started.");

                            exe.started = true;
                            if (exe.background)
                                exe.LogBackgroundStart();

                            try
                            {
                                exe.command.action(exe);

                                if (exe.error != null)
                                    if (exe.line.HasFlags_any(SIGNALS.EXEC | SIGNALS.TICK))
                                        error = $"{this} {exe} {exe.error}";
                            }
                            catch (Exception e)
                            {
                                Debug.LogException(e);
                            }

                            exe.Dispose();
                        }

                    if (exe.routine != null)
                    {
                        if (line.HasFlags_any(SIGNALS.EXEC | SIGNALS.TICK))
                        {
                            if (!exe.started && exe.background)
                                exe.LogBackgroundStart();
                            exe.started = true;
                        }

                        try
                        {
                            bool has_next = exe.routine.MoveNext();

                            if (exe.error != null)
                                if (exe.line.HasFlags_any(SIGNALS.EXEC | SIGNALS.TICK))
                                {
                                    error = $"{this} {exe} {Util.PullValue(ref exe.error)}";
                                    Debug.LogWarning(error);
                                    exe.Dispose();
                                }
                                else
                                    exe.error = null;

                            if (!has_next)
                                exe.Dispose();
                        }
                        catch (Exception e)
                        {
                            Debug.LogException(e);
                            exe.Dispose();
                        }
                    }

                    exe.line = null;
                    return true;
                }

                //--------------------------------------------------------------------------------------------------------------

                public void Dispose()
                {
                    disposed = true;
                    for (int i = 0; i < _executors.Count; i++)
                        _executors[i].Janitize();
                    _executors.Clear();
                    temp_vars.Dispose();
                }
            }
        }
    }
}