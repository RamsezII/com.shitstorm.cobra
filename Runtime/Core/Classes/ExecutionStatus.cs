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
    public readonly struct ExecutionStatus
    {
        public readonly CMD_STATUS code;
        public readonly LintedString prefixe;
        public readonly float progress;
        public readonly string error;

        //----------------------------------------------------------------------------------------------------------

        public ExecutionStatus(in CMD_STATUS code = 0, in LintedString prefixe = default, in float progress = 0, in string error = null)
        {
            this.code = code;
            this.prefixe = prefixe;
            this.progress = progress;
            this.error = error;
        }
    }
}