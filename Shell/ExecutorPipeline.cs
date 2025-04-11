using System;
using System.Collections.Generic;
using UnityEngine;

namespace _COBRA_
{
    internal class ExecutorPipeline : IDisposable
    {
        readonly List<Command.Executor> _executors = new();
        bool disposed;

        //--------------------------------------------------------------------------------------------------------------

        internal ExecutorPipeline(in Command.Executor executor) => AddExecutor(executor);

        //--------------------------------------------------------------------------------------------------------------

        internal void AddExecutor(in Command.Executor executor)
        {
            if (disposed)
                Debug.LogError($"adding {executor.GetType().FullName} '{executor.command.name}' ({executor.cmd_path}) to a disposed {GetType().FullName}.");
            executor.pipeline = this;
            _executors.Remove(executor);
            _executors.Add(executor);
        }

        internal bool TryExecuteCurrent(in Command.Line line, out Command.Executor executor)
        {
            if (!TryGetCurrent(out executor))
            {
                Dispose();
                return false;
            }

            executor.line = line;

            if (executor.command.action != null)
                if (line.HasFlags_any(SIGNAL_FLAGS.EXEC | SIGNAL_FLAGS.TICK))
                {
                    executor.command.action(executor);
                    executor.Dispose();
                }

            if (executor.routine != null)
                if (line.signal.HasFlag(SIGNAL_FLAGS.TICK))
                    if (!executor.routine.MoveNext())
                        executor.Dispose();

            executor.line = null;

            return true;
        }

        internal bool TryGetCurrent(out Command.Executor executor)
        {
            for (int i = 0; i < _executors.Count; i++)
                if (!_executors[i].disposed.Value)
                {
                    executor = _executors[i];
                    return true;
                }
            executor = null;
            return false;
        }

        //--------------------------------------------------------------------------------------------------------------

        internal bool AreAllDisposed()
        {
            for (int i = 0; i < _executors.Count; i++)
                if (!_executors[i].disposed.Value)
                    return false;
            return true;
        }

        //--------------------------------------------------------------------------------------------------------------

        public void Dispose()
        {
            disposed = true;
            for (int i = 0; i < _executors.Count; i++)
                _executors[i].PropagateDispose();
            _executors.Clear();
        }
    }
}