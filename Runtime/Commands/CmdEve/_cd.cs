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
                    eve_path_list.Add(arg);
                    break;
            }
        }
    }
}