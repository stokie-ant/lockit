using Microsoft.Win32;

namespace lockit
{
    internal class Regedits
    {
        public static void NoLogoff(bool state)
        {
            try
            {
                RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", true);
                key.SetValue("NoLogoff", state, RegistryValueKind.DWord);
                key.Close();
            }
            catch { } // fuck it, should be admin anyway
        }

        public static void NoSwitchUser(bool state)
        {
            try
            {
                RegistryKey key = Registry.LocalMachine.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System", true);
                key.SetValue("HideFastUserSwitching", state, RegistryValueKind.DWord);
                key.Close();
            }
            catch { } // see previous comment
        }

        public static void NoPowerMenu(bool state)
        {
            try
            {
                RegistryKey key = Registry.LocalMachine.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", true);
                key.SetValue("NoClose", state, RegistryValueKind.DWord);
                key.Close();
            }
            catch { } // yeah
        }
    }
}
