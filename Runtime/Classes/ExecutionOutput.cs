using System;

namespace _COBRA_
{
    public enum CMD_STATUS : byte
    {
        BLOCKED,
        WAIT_FOR_STDIN,
        NETWORKING,
        RETURN,
        ERROR,
    }

    [Serializable]
    public readonly struct ExecutionOutput
    {
        public readonly CMD_STATUS status;
        public readonly LintedString prefixe;
        public readonly float progress;
        public readonly string error;

        //----------------------------------------------------------------------------------------------------------

        public ExecutionOutput(in CMD_STATUS status = 0, in LintedString prefixe = default, in float progress = 0, in string error = null)
        {
            this.status = status;
            this.prefixe = prefixe.text == null ? LintedString.EMPTY : prefixe;
            this.progress = progress;
            this.error = error;
        }
    }
}