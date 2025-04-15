using _UTIL_;
using System;
using System.Collections.Generic;
using System.Linq;

namespace _COBRA_
{
    partial class CmdEve
    {
        static void ChangeDirectory(Command.Executor eve_exe, Command.Executor cmd_exe, string current_path, List<string> eve_path_list)
        {
            string arg = (string)cmd_exe.args[0];
            switch (arg)
            {
                case "..":
                    if (eve_path_list.Count() > 0)
                        eve_path_list.RemoveAt(eve_path_list.Count - 1);
                    break;

                default:
                    if (!eve_tree.TryGetValue(current_path, out NGinxIndex index))
                        ;
                    else
                    {
                        NGinxIndex.Entry entry = index.entries.FirstOrDefault(entry => entry.name.Equals(arg, StringComparison.OrdinalIgnoreCase));
                        if (entry != null)
                            eve_path_list.Add(entry.name);
                    }
                    break;
            }
        }
    }
}