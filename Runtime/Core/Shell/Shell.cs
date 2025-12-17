using _ARK_;
using _UTIL_;
using System;

namespace _COBRA_
{
    public abstract partial class Shell : Disposable
    {
        public readonly ValueHandler<ExecutionStatus> status = new();
        public Action<object, string> on_output;

        //----------------------------------------------------------------------------------------------------------

        public virtual void Init()
        {
            status.Value = RegularStatus();
            Util.AddAction(ref NUCLEOR.delegates.Update_OnShellTick, OnTick);
        }

        //----------------------------------------------------------------------------------------------------------

        public abstract void OnReader(in CodeReader reader);

        protected abstract void OnTick();

        //----------------------------------------------------------------------------------------------------------

        protected override void OnDispose()
        {
            NUCLEOR.delegates.Update_OnShellTick -= OnTick;

            base.OnDispose();

            status.Dispose();
        }
    }
}