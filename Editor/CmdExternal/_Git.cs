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
                max_args: 4,
                opts: static exe => exe.line.TryReadOption_workdir(exe),
                args: static exe =>
                {
                    if (exe.line.TryReadArgument(out string subcommand, out bool is_valid, new string[] { "status", "add-all", "commit", "push", "pull", "fetch", "clone", }))
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

                                case "clone":
                                    if (exe.line.TryReadArgument(out string author_name, out _))
                                    {
                                        exe.args.Add(author_name);
                                        if (exe.line.TryReadArgument(out string repo_name, out _))
                                        {
                                            exe.args.Add(repo_name);
                                            if (exe.line.TryReadArgument(out string local_name, out _))
                                                exe.args.Add(local_name);
                                        }
                                        else
                                            exe.error = $"specify {nameof(repo_name)}";
                                    }
                                    else
                                        exe.error = $"please specify {nameof(author_name)}";
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

                        case "clone":
                            {
                                string author_name = (string)exe.args[1];
                                string repo_name = (string)exe.args[2];
                                string local_name = exe.args.Count == 2 ? repo_name : (string)exe.args[3];

                                string url = $"git@github.com:{author_name}/{repo_name}.git {local_name}";

                                input += "clone " + url;
                            }
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