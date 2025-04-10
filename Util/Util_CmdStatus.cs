using System;
using UnityEngine;

namespace _COBRA_
{
    public enum CMD_STATES : byte
    {
        BLOCKING,
        WAIT_FOR_STDIN,
        FULLSCREEN_readonly,
        FULLSCREEN_write,
    }

    public struct CMD_STATUS
    {
        public CMD_STATES state;
        public string prefixe;
        public bool immortal;
        [Range(0, 1)] public float progress;
        public string error;

        //--------------------------------------------------------------------------------------------------------------

        public CMD_STATUS(in CMD_STATES state,
            in string prefixe = null,
            in bool immortal = false,
            in float progress = 0,
            in string error = null)
        {
            this.state = state;
            this.prefixe = prefixe;
            this.immortal = immortal;
            this.progress = progress;
            this.error = error;
        }
    }
}