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

        // for d in */; do (cd "$d" && [ -d .git ] && echo "➡️ $d" && git pull); done
        // for d in */; do (cd "$d" && [ -d .git ] && echo "➡️ $d" && git reset --hard); done

        // for d in */; do
        // if [ -d "$d/.git" ]; then
        //     echo -e "\n\e[1;36m📂 $d\e[0m"
        //     (cd "$d" && git pull)
        // fi
        // done

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