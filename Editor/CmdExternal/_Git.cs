using System;
using System.Collections.Generic;
using System.IO;
using _COBRA_;

namespace _COBRA_e
{
    partial class CmdExternal
    {
        /*
            git
            ├── init
            ├── clone [url]
            ├── status
            ├── add [file...]
            ├── commit [-m "message"]
            ├── checkout [branch]
            ├── push
            ├── pull
            ├── merge [branch]
            └── branch
                ├── -d [branch]
                └── [new-branch]
        */

        //--------------------------------------------------------------------------------------------------------------

        static void Init_Git()
        {
            Command cmd_git = Command.static_domain.AddDomain("git");

            cmd_git.AddAction(
                "status",
                action: static exe =>
                {

                }
            );
        }
    }
}