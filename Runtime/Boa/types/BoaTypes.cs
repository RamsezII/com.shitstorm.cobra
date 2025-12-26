using System;
using System.Collections.Generic;
using UnityEngine;

namespace _COBRA_.Boa
{
    public static class BoaTypes
    {
        public static readonly Dictionary<string, Type> types = new(StringComparer.OrdinalIgnoreCase);

        //----------------------------------------------------------------------------------------------------------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            types.Clear();

            types.Add("bool", typeof(bool));
            types.Add("int", typeof(int));
            types.Add("float", typeof(float));
            types.Add("string", typeof(string));
        }
    }
}