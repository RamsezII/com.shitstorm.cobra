using _ARK_;
using _UTIL_;
using System;

namespace _COBRA_
{
    public abstract partial class Shell : Disposable
    {
        public readonly ValueHandler<CMD_STATUS> status = new();
        public readonly ValueHandler<LintedString> prefixe = new();
        public Action<object, string> on_output;

        //----------------------------------------------------------------------------------------------------------

        protected Shell()
        {
            prefixe.Value = RegularPrefixe();
        }

        //----------------------------------------------------------------------------------------------------------

        public virtual void Init()
        {
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
            prefixe.Dispose();
        }
    }
}