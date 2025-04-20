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
            Command.static_domain.AddAction(
                "git",
                min_args: 1,
                max_args: 2,
                opts: static exe => exe.line.TryReadOption_workdir(exe),
                args: static exe =>
                {
                    if (exe.line.TryReadArgument(out string subcommand, out bool is_valid, new string[] { "status", "add-all", "commit", "push", "pull", "fetch", }))
                        if (is_valid)
                        {
                            subcommand = subcommand.ToLower();
                            exe.args.Add(subcommand);
                            switch (subcommand)
                            {
                                case "commit":
                                    if (exe.line.TryReadArgument(out string commit_msg, out _))
                                        exe.args.Add(commit_msg);
                                    break;
                            }
                        }
                },
                action: static exe =>
                {
                    string subcommand = (string)exe.args[0];

                    string input = $"git ";

                    switch (subcommand)
                    {
                        case "commit":
                            input += $"-m \"{exe.args[1]}\"";
                            break;

                        case "add-all":
                            input += "add .";
                            break;

                        default:
                            input += subcommand;
                            break;
                    }

                    string workdir = exe.GetWorkdir();
                    Util.RunExternalCommand(workdir, input, stdout => exe.Stdout(stdout));
                });
        }
    }
}