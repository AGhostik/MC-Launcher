using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Launcher
{
    static class Program
    {
        public static Form1 form1;
        public static Form2 form2;
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]

        static int Main(String[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            form2 = new Form2();
            Application.Run(form1 = new Form1());

            return 0;
        }
    }
    public static class Global
    {
        public static string Directory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\minecraft";
        public static string JavaPath = String.Empty;
        public static string Nick = String.Empty;
        public static string Version = String.Empty;
        public static string VersionN = String.Empty;
        public static string Visib = String.Empty;
        public static string Latest = String.Empty;
        public static string LatestSnap = String.Empty;
        public static string[] Releases = new string[1024];
        public static string[] Snapshots = new string[1024];
        public static string[] Beta = new string[1024];
        public static string[] Alpha = new string[1024];
        public static string[] Uncommon = new string[1024];
        public static string StartLibraries = String.Empty;
        public static string MainClass = String.Empty;
        public static string MinecraftArg = String.Empty;
        public static string AssetsIndex = String.Empty;
        public static string LauncherVersion = "v. [13.03.16]";
        public static int LauncherCheckVersion; //сверка версии лаунчера с версией на сайте
        public static int ReleaseInt = 0;
        public static int SnapshotInt = 0;
        public static int BetaInt = 0;
        public static int AlphaInt = 0;
        public static int UncommonInt = 0;
        public static string Memory = "-Xincgc -Xms512M -Xmx1024M";
        public static bool RelCheck = true;
        public static bool SnapCheck = false;
        public static bool BetaCheck = false;
        public static bool AlphaCheck = false;
        public static bool UncommonCheck = false;
        public static System.Drawing.Bitmap headFront, handFront, bodyFront, legFront, headBack, bodyBack, handBack, legBack, helmFront, helmBack, cloak;
        public static string CloakPath = String.Empty, SkinPath = String.Empty;
        public static int Vis = 0;
        public static int Opt = 0;
        public static int Skn = 0;
    }
}
