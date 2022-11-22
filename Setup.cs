using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;

namespace lockit
{
    public partial class Setup : Form
    {
        public Setup()
        {
            InitializeComponent();
        }

        private void SaveAndLockButton_Click(object sender, EventArgs e)
        {
            if (!ConfirmPasswordMatch())
            {
                System.Windows.MessageBox.Show
                    ("Passwords dont match", "Passwords dont match", MessageBoxButton.OK);
                return;
            }
            SetPassword();
            SaveConfig();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void SaveAndExitButton_Click(object sender, EventArgs e)
        {
            if (!ConfirmPasswordMatch())
            {
                System.Windows.MessageBox.Show
                    ("Passwords dont match", "Passwords dont match", MessageBoxButton.OK);
                return;
            }
            SetPassword();
            SaveConfig();
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private bool ConfirmPasswordMatch()
        {
            if (PasswordTextBox.Text == ConfirmPasswordTextBox.Text)
                return true;
            return false;
        }

        private void SetPassword()
        {
            if (string.IsNullOrEmpty(Lockit.Conf.Password)) // password never set before
            {
                Lockit.Conf.Salt = Crypt.GetSaltBytes();
                Lockit.Conf.Password = Crypt.GetHash(PasswordTextBox.Text, Lockit.Conf.Salt);
            }
            else // password has been set before
            {
                if (!string.IsNullOrEmpty(PasswordTextBox.Text)) // if the box is empty don't change
                {
                    Lockit.Conf.Salt = Crypt.GetSaltBytes();
                    Lockit.Conf.Password = Crypt.GetHash(PasswordTextBox.Text, Lockit.Conf.Salt);
                }
            }
        }

        private void SaveConfig()
        {
            Lockit.Conf.RemoveSignOut = RemoveSignOutcheckBox.Checked;
            Lockit.Conf.RemoveSwitchUser = RemoveSwitchUserCheckBox.Checked;
            Lockit.Conf.RemovePowerMenu = RemovePowerMenuCheckBox.Checked;
            Lockit.Conf.KeepRemoveSignout = KeepRemoveSignOutcheckBox.Checked;
            Lockit.Conf.KeepRemoveSwitchUser = KeepRemoveSwitchUserCheckBox.Checked;
            Lockit.Conf.KeepRemovePowerMenu = KeepRemovePowerMenuCheckBox.Checked;

            if (ColourRadioButton.Checked)
            {
                Lockit.Conf.Backtype = "Colour";
                Lockit.Conf.BackImage = "";
            }
            else if (ImageRadioButton.Checked)
            {
                Lockit.Conf.Backtype = "Image";
                Lockit.Conf.BackColour = "";
                Lockit.Conf.BackImage = ImageSelectTextBox.Text;

                if (ImageBackgroundColourRadioButton.Checked)
                    Lockit.Conf.ImageBackType = "Colour";
                else
                    Lockit.Conf.ImageBackType = "Transparent";
            }
            else if (TransparentRadioButton.Checked)
            {
                Lockit.Conf.Backtype = "Transparent";
                Lockit.Conf.BackColour = "";
                Lockit.Conf.BackImage = "";
            }
            Lockit.Conf.WriteConfig();
            try
            {
                Shortcuts.CreateShortcut(AppDomain.CurrentDomain.BaseDirectory,
                    "Lockit Setup",
                    AppDomain.CurrentDomain.BaseDirectory,
                    Path.GetFileName(Assembly.GetEntryAssembly().Location),
                    "Lockit Setup",
                    "setup");
            }
            catch { }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void Settings_Shown(object sender, EventArgs e)
        {
            ShortcutsLocationTextBox.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            switch (Lockit.Conf.Backtype)
            {
                case "Colour":
                    ColourRadioButton.Checked = true;
                    groupBox4.Enabled = false;
                    ImageSelectTextBox.Enabled = false;
                    ColourButton.Enabled = true;
                    break;
                case "Image":
                    ImageRadioButton.Checked = true;
                    ColourButton.Enabled = false;
                    groupBox4.Enabled = true;
                    ImageSelectTextBox.Text = Lockit.Conf.BackImage;

                    switch (Lockit.Conf.ImageBackType)
                    {
                        case "Transparent":
                            ImageBackgroundTransparentRadioButton.Checked = true;
                            break;
                        default:
                            ImageBackgroundColourRadioButton.Checked = true;
                            BackgroundImageColourSelectButton.Enabled = true;
                            break;
                    }
                    break;
                default:
                    TransparentRadioButton.Checked = true;
                    ColourButton.Enabled = false;
                    groupBox4.Enabled = false;
                    ImageSelectTextBox.Enabled = false;
                    break;
            }

            AsAdminLabel.Text = Lockit.LaunchedAsAdmin ? "Running as Administrator" : "Not running as Administrator";

            InstallUACBypassButton.Text = TaskSched.IsUACbypassEnabled() ? "Remove" : "Install";
            UACBypassEnabledLabel.Text = TaskSched.IsUACbypassEnabled() ? "UAC bypass enabled" : "UAC bypass not enabled";

            if (!Lockit.LaunchedAsAdmin)
            {
                RemoveSignOutcheckBox.Checked = Lockit.Conf.RemoveSignOut;
                RemoveSwitchUserCheckBox.Checked = Lockit.Conf.RemoveSwitchUser;
                RemovePowerMenuCheckBox.Checked = Lockit.Conf.RemovePowerMenu;
                RemoveSignOutcheckBox.Enabled = false;
                RemoveSwitchUserCheckBox.Enabled = false;
                RemovePowerMenuCheckBox.Enabled = false;
                KeepRemoveSignOutcheckBox.Enabled = false;
                KeepRemoveSwitchUserCheckBox.Enabled = false;
                KeepRemovePowerMenuCheckBox.Enabled = false;
            }

            KeepRemoveSignOutcheckBox.Checked = Lockit.Conf.KeepRemoveSignout;
            KeepRemoveSwitchUserCheckBox.Checked = Lockit.Conf.KeepRemoveSwitchUser;
            KeepRemovePowerMenuCheckBox.Checked = Lockit.Conf.KeepRemovePowerMenu;
        }

        private void ColourButton_Click(object sender, EventArgs e)
        {
            ColorDialog Cd = new ColorDialog();
            if (!string.IsNullOrEmpty(Lockit.Conf.BackColour))
                Cd.Color = System.Drawing.Color.FromArgb(int.Parse(Lockit.Conf.BackColour));
            if (Cd.ShowDialog() == DialogResult.OK)
            {
                Lockit.Conf.BackColour = Cd.Color.ToArgb().ToString();

            }
        }

        private void TransparentRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            ColourButton.Enabled = false;
            ImageSelectTextBox.Enabled = false;
            ImageButton.Enabled = false;
            groupBox4.Enabled = false;

        }

        private void ColourRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            ColourButton.Enabled = true;
            ImageSelectTextBox.Enabled = false;
            ImageButton.Enabled = false;
            groupBox4.Enabled = false;

        }

        private void ImageRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            ColourButton.Enabled = false;
            ImageSelectTextBox.Enabled = true;
            ImageButton.Enabled = true;
            groupBox4.Enabled = true;

        }

        private void ImageButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            {
                openFileDialog1.Filter = "Images (*.BMP;*.JPG;*.GIF,*.PNG,*.TIFF)|*.BMP;*.JPG;*.GIF;*.PNG;*.TIFF";
            }
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                ImageSelectTextBox.Text = openFileDialog1.FileName;
            }
        }

        private void BackgroundImageColourSelectButton_Click(object sender, EventArgs e)
        {
            ColorDialog Cd = new ColorDialog();
            if (!string.IsNullOrEmpty(Lockit.Conf.ImageBackColour))
                Cd.Color = System.Drawing.Color.FromArgb(int.Parse(Lockit.Conf.ImageBackColour));
            if (Cd.ShowDialog() == DialogResult.OK)
            {
                Lockit.Conf.ImageBackColour = Cd.Color.ToArgb().ToString();
            }
        }

        private void ImageBackgroundColourRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            BackgroundImageColourSelectButton.Enabled = true;
        }

        private void ImageBackgroundTransparentRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            BackgroundImageColourSelectButton.Enabled = false;
        }

        private void MakeStartMenuShortcutsButton_Click(object sender, EventArgs e)
        {
            string StartMenuPath = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);
            Lockit.MakeShortCutsInPath(StartMenuPath + "\\Lockit");
        }

        private void MakeShortcutsInLocationButton_Click(object sender, EventArgs e)
        {
            if (ShortcutsLocationTextBox.Text == "")
                ShortcutsLocationTextBox.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            Lockit.MakeShortCutsInPath(ShortcutsLocationTextBox.Text);
        }

        private void InstallUACBypassButton_Click(object sender, EventArgs e)
        {
            if (!Lockit.LaunchedAsAdmin)
            {
                if (InstallUACBypassButton.Text == "Install")
                {
                    TryRunAdmin("setupuacbypass");
                }
                else
                {
                    TryRunAdmin("removeuacbypass");
                }

            }
            else
            {
                if (InstallUACBypassButton.Text == "Install")
                {
                    TaskSched.CreateUACBypass(Assembly.GetEntryAssembly().Location, "", "LockitUACBypass");
                    TaskSched.CreateUACBypass(Assembly.GetEntryAssembly().Location, "Setup", "LockitUACBypassSetup");
                }
                else
                {
                    TaskSched.RemoveUACBypass();
                }
            }
            InstallUACBypassButton.Text = TaskSched.IsUACbypassEnabled() ? "Remove" : "Install";
            UACBypassEnabledLabel.Text = TaskSched.IsUACbypassEnabled() ? "UAC bypass enabled" : "UAC bypass not enabled";
        }

        private void TryRunAdmin(string args = "")
        {
            if (System.Windows.MessageBox.Show
                    ("Restart as administrator?", "Restart?", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                ProcessStartInfo proc = new ProcessStartInfo
                {
                    UseShellExecute = true,
                    WorkingDirectory = Environment.CurrentDirectory,
                    FileName = Assembly.GetEntryAssembly().CodeBase,
                    Arguments = args,
                    Verb = "runas"
                };
                try
                {
                    Process.Start(proc);
                    Environment.Exit(0);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("This program must be run as an administrator! \n\n" + ex.ToString());
                    Environment.Exit(0);
                }
            }
        }

        private void TryRestartAsAdminButton_Click(object sender, EventArgs e)
        {
            TryRunAdmin("SETUP");
        }

        private void PasswordTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (PasswordTextBox.Text == ConfirmPasswordTextBox.Text)
            {
                PasswordTextBox.BackColor = System.Drawing.Color.PaleGreen;
                ConfirmPasswordTextBox.BackColor = System.Drawing.Color.PaleGreen;
            }
            else
            {
                PasswordTextBox.BackColor = System.Drawing.Color.Pink;
                ConfirmPasswordTextBox.BackColor = System.Drawing.Color.Pink;
            }
        }

        private void ShowPasswordcheckBox_Click(object sender, EventArgs e)
        {
            PasswordTextBox.UseSystemPasswordChar = !ShowPasswordcheckBox.Checked;
            ConfirmPasswordTextBox.UseSystemPasswordChar = !ShowPasswordcheckBox.Checked;
        }
    }
}
