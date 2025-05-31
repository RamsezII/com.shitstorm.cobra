using System;
using UnityEngine;

namespace _COBRA_
{
    partial class Shell
    {
        public readonly struct RunInfos : IEquatable<RunInfos>
        {
            public readonly ushort eid;
            public readonly CMD_STATUS status;

            //--------------------------------------------------------------------------------------------------------------

            public RunInfos(in ushort eid, in CMD_STATUS status)
            {
                this.eid = eid;
                this.status = status;
            }

            //--------------------------------------------------------------------------------------------------------------

            public bool Equals(RunInfos other)
            {
                if (eid != other.eid || status.state != other.status.state)
                    return false;

                if (status.prefixe == null && other.status.prefixe == null)
                    return true;

                if (status.prefixe == null || other.status.prefixe == null)
                    return false;

                return status.prefixe.Equals(other.status.prefixe, StringComparison.Ordinal);
            }
        }

        public RunInfos current_state, previous_state;

        //--------------------------------------------------------------------------------------------------------------

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Assets/" + nameof(_COBRA_) + "/" + nameof(Shell) + "/" + nameof(_Test))]
        static void _Test()
        {
            RunInfos a = new(3, new CMD_STATUS(0));
            RunInfos b = new(3, new CMD_STATUS(0));
            Debug.Log(a.Equals(b));
        }
#endif
    }
}