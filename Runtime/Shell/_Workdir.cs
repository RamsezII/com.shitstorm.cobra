using _ARK_;
using System.IO;

namespace _COBRA_
{
    partial class Shell
    {
        public static readonly string referent_dir = Directory.GetCurrentDirectory();
        public string work_dir;

        //--------------------------------------------------------------------------------------------------------------

        void AwakeWorkDir()
        {
            work_dir = NUCLEOR.home_path;
        }

        //--------------------------------------------------------------------------------------------------------------

        public string GetPrefixe(string user_name = null, string cmd_path = null)
        {
            string work_dir = this.work_dir;

            if (Util.Equals_path(work_dir, referent_dir))
                work_dir = "~";
            else if (work_dir.Contains(referent_dir))
                work_dir = Path.Combine("~", Path.GetRelativePath(referent_dir, work_dir));

            user_name ??= MachineSettings.machine_name.Value;
            cmd_path ??= work_dir;
            return $"{user_name.SetColor("#73CC26")}:{cmd_path.SetColor("#73B2D9")}$";
        }
    }
}