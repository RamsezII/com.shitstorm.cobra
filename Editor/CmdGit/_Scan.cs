using System.IO;
using _COBRA_;
using UnityEngine;

namespace _COBRA_e
{
    partial class GitCmd
    {
        static void Init_Scan()
        {
            Command.static_domain.AddAction(
                "git-scan",
                action: static exe =>
                {
                    foreach (string dir in Directory.EnumerateDirectories(Application.dataPath))
                        if (Directory.Exists(Path.Combine(dir, ".git")))
                        {
                            Debug.Log("git ici: " + dir);
                        }
                }
            );
        }
    }
}