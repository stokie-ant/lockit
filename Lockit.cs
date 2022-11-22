using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using System.Windows.Forms;

namespace lockit
{
    struct Config
    {
        public string Backtype;
        public string BackColour;
        public string BackImage;
        public string ImageBackType;
        public string ImageBackColour;
        public string Salt;
        public string Password;
        public bool RemoveSignOut;
        public bool RemoveSwitchUser;
        public bool RemovePowerMenu;
        public bool KeepRemoveSignout;
        public bool KeepRemoveSwitchUser;
        public bool KeepRemovePowerMenu;

        public bool ReadConfig()
        {
            string parseString(string Item, string[] FileLines)
            {
                if (Array.FindIndex(FileLines, s => s.StartsWith(Item, true, CultureInfo.InvariantCulture)) > -1)
                {
                    string Line = FileLines[Array.FindIndex(FileLines, s => s.StartsWith(Item, true, CultureInfo.InvariantCulture))];
                    if (!string.IsNullOrEmpty(Line))
                    {
                        string[] LineArray = Line.Split(new char[] { '=' }, 2, StringSplitOptions.None);
                        if (LineArray.Length > 1)
                            return LineArray[1];
                    }
                    return "";
                }
                return "";
            }

            bool ParseBool(string Item, string[] FileLines)
            {
                if (Array.FindIndex(FileLines, s => s.StartsWith(Item, true, CultureInfo.InvariantCulture)) > -1)
                {
                    string Line = FileLines[Array.FindIndex(FileLines, s => s.StartsWith(Item, true, CultureInfo.InvariantCulture))];
                    if (!string.IsNullOrEmpty(Line))
                    {
                        string[] LineArray = Line.Split(new char[] { '=' }, 2, StringSplitOptions.None);
                        if (LineArray.Length > 1)
                        {
                            return bool.Parse(LineArray[1]);
                        }

                    }
                    return false;
                }
                return false;
            }


            if (File.Exists("Lockit.conf"))
            {
                string[] Lines = File.ReadAllLines("Lockit.conf");
                Backtype = parseString("BackType=", Lines);
                BackColour = parseString("BackColour=", Lines);
                BackImage = parseString("BackImage=", Lines);
                ImageBackType = parseString("ImageBackType=", Lines);
                ImageBackColour = parseString("ImageBackColour=", Lines);
                Salt = parseString("Salt=", Lines);
                Password = parseString("Password=", Lines);
                RemoveSignOut = ParseBool("RemoveSignOut=", Lines);
                RemoveSwitchUser = ParseBool("RemoveSwitchUser=", Lines);
                RemovePowerMenu = ParseBool("RemovePowerMenu=", Lines);
                KeepRemoveSignout = ParseBool("KeepRemoveSignout=", Lines);
                KeepRemoveSwitchUser = ParseBool("KeepRemoveSwitchUser=", Lines);
                KeepRemovePowerMenu = ParseBool("KeepRemovePowerMenu=", Lines);
                return true;
            }
            return false;
        }

        public bool WriteConfig()
        {
            try
            {
                string[] lines =
                {
                    "BackType="+Backtype,
                    "BackColour="+BackColour,
                    "BackImage="+BackImage,
                    "ImageBackType="+ImageBackType,
                    "ImageBackColour="+ImageBackColour,
                    "Salt="+Salt,
                    "Password="+Password,
                    "RemoveSignOut="+ RemoveSignOut.ToString(),
                    "RemoveSwitchUser="+RemoveSwitchUser.ToString(),
                    "RemovePowerMenu="+RemovePowerMenu.ToString(),
                    "KeepRemoveSignout="+KeepRemoveSignout.ToString(),
                    "KeepRemoveSwitchUser="+KeepRemoveSwitchUser.ToString(),
                    "KeepRemovePowerMenu="+KeepRemovePowerMenu.ToString()
                };

                File.WriteAllLines("Lockit.conf", lines);
                return true;
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show(ex.Message);
                return false;
            }
        }
    }

    internal static class Lockit
    {
        public static Config Conf = new Config();
        public static bool LaunchedAsAdmin = false;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static int Main()
        {
            string[] args = Environment.GetCommandLineArgs();

            var id = WindowsIdentity.GetCurrent();
            if (id.Owner == id.User)
            {
                LaunchedAsAdmin = false;
                if (TaskSched.IsUACbypassEnabled())
                {
                    string schargs = "/run /tn LockitUACBypass";

                    if (args.Length > 1)
                        if (args[1].ToUpper() == "SETUP")
                            schargs = "/run /tn LockitUACBypassSetup";

                    Process proc = new Process();
                    proc.StartInfo.FileName = "schtasks";
                    proc.StartInfo.Arguments = schargs;
                    proc.StartInfo.CreateNoWindow = true;
                    proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    Process.Start(proc.StartInfo);
                    return 0;
                }
            }
            else
            {
                LaunchedAsAdmin = true;
            }

            if (args.Length > 1)
            {
                if (args[1].ToUpper() == "SETUPUACBYPASS")
                {
                    if (LaunchedAsAdmin)
                    {
                        TaskSched.CreateUACBypass(Assembly.GetEntryAssembly().Location, "", "LockitUACBypass");
                        TaskSched.CreateUACBypass(Assembly.GetEntryAssembly().Location, "Setup", "LockitUACBypassSetup");
                    }
                    else
                    {
                        MessageBox.Show("Strange.. We should be Admin now but we aren't");
                        return 1;
                    }
                    Conf.ReadConfig();
                }
                else if (args[1].ToUpper() == "REMOVEUACBYPASS")
                {
                    if (LaunchedAsAdmin)
                    {
                        TaskSched.RemoveUACBypass();
                    }
                    else
                    {
                        MessageBox.Show("Strange.. We should be Admin now but we aren't");
                        return 1;
                    }
                    Conf.ReadConfig();
                }
                else if (args[1].ToUpper() == "SETUP")
                {
                    Conf.ReadConfig();
                }
                else // bad args
                {
                    MessageBox.Show("wrong argument or number of arguments");
                    return 1;
                }
            }
            else
                Conf.ReadConfig(); // read config and lock if it exists. if not, show setup

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Interface());
            return 0;
        }

        public static void MakeShortCutsInPath(string location)
        {
            try
            {
                Directory.CreateDirectory(location);
                string OurPath = AppDomain.CurrentDomain.BaseDirectory;
                string OurName = Path.GetFileName(Assembly.GetEntryAssembly().Location);

                Shortcuts.CreateShortcut(location,
                    "Lockit Setup",
                    AppDomain.CurrentDomain.BaseDirectory,
                    Path.GetFileName(Assembly.GetEntryAssembly().Location),
                    "Lockit Setup",
                    "setup");

                Shortcuts.CreateShortcut(location,
                    "Lockit",
                    AppDomain.CurrentDomain.BaseDirectory,
                    Path.GetFileName(Assembly.GetEntryAssembly().Location),
                    "Lockit");
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show(ex.Message);
            }
        }
    }
}
