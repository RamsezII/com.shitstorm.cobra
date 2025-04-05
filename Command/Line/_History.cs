using _ARK_;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace _COBRA_
{
    partial class Command
    {
        partial class Line
        {
            static readonly string history_file_name = typeof(Line).TypeToFileName() + ".history.txt";
            static string GetHistoryPath() => Path.Combine(NUCLEOR.home_path.ForceDirPath(), history_file_name);

            const int max_history = 50;
            static readonly List<string> history = new(max_history);
            static int history_index;
            public static void ResetHistoryCount() => history_index = 0;
            public static void ClearHistory() => history.Clear();

            //--------------------------------------------------------------------------------------------------------------

            static void SaveHistory()
            {
                if (history.Count == 0)
                    return;

                string path = GetHistoryPath();
                using StreamWriter writer = new(path, false);

                foreach (string line in history)
                    writer.WriteLine(line);
            }

            static void LoadHistory()
            {
                history.Clear();

                string path = GetHistoryPath();

                if (File.Exists(path))
                    foreach (string line in File.ReadLines(path))
                        AddToHistory(line);
            }

            //--------------------------------------------------------------------------------------------------------------

            public static void AddToHistory(in string entry)
            {
                if (history.Contains(entry))
                    history.Remove(entry);
                else if (history.Count >= max_history)
                    history.RemoveAt(0);
                history.Add(entry);
            }

            public static bool OnHistoryNav(in KeyCode key, out string entry)
            {
                if (history.Count == 0)
                {
                    entry = string.Empty;
                    return false;
                }

                int increment = key switch
                {
                    KeyCode.UpArrow => -1,
                    KeyCode.DownArrow => 1,
                    _ => 0
                };

                history_index += increment;
                if (history_index < 0)
                    history_index += history.Count;
                history_index %= 1 + history.Count;

                if (history_index < history.Count)
                {
                    entry = history[history_index];
                    return true;
                }

                entry = string.Empty;
                return true;
            }
        }
    }
}