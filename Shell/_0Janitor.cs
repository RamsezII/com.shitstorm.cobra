#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace _COBRA_
{
    partial class Shell
    {
        [SerializeField, TextArea(1, _max_janitized)] string _janitor;
        readonly Queue<string> _janitor_queue = new();

        const int _max_janitized = 50;

        //--------------------------------------------------------------------------------------------------------------

        public void Janitize(in object line)
        {
            while (_janitor_queue.Count > _max_janitized)
                _janitor_queue.Dequeue();
            _janitor_queue.Enqueue(line.ToString());
            _janitor = string.Join("\n", _janitor_queue);
        }
    }
}
#endif