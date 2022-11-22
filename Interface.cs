using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace lockit
{
    public partial class Interface : Form
    {
        private int LockoutDelay = 1; // seconds exponential start
        private Form[] SecondaryForms = { new Form() }; // for forms to cover secondary screens
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TOPMOST = 0x08;
        private bool CanClose = false;

        public Interface()
        {
            InitializeComponent();
            dStayOnTop = new DelegateStayOnTop(StayOnTop); // for StayOnTopThread

            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1) // argument validation is done in main
            {
                if (args[1].ToUpper() == "SETUPUACBYPASS" ||
                     args[1].ToUpper() == "REMOVEUACBYPASS" ||
                     args[1].ToUpper() == "SETUP")
                {
                    DoSettings();
                }
            }
            else if (File.Exists("Lockit.conf"))
            {
                Lockdown();
            }
            else
            {
                DoSettings();
            }
        }

        private void DoSettings()
        {
            var Settings = new Setup();
            if (Settings.ShowDialog() != DialogResult.OK)
            {
                CanClose = true;
            }
            else
            {
                Lockdown();
            }
        }

        private async void OKButton_Click(object sender, EventArgs e)
        {
            if (Crypt.GetHash(textBox1.Text, Lockit.Conf.Salt) == Lockit.Conf.Password) // password success
            {
                FadeOut(this);

                Close();
            }
            else // wrong password time penalty
            {
                textBox1.Text = "";
                label1.Text = "Wrong Password";
                textBox1.Enabled = false;
                textBox1.UseSystemPasswordChar = false;
                button1.Enabled = false;
                LockoutDelay *= 2;
                await LockOut(LockoutDelay);
                textBox1.Text = "";
                textBox1.UseSystemPasswordChar = true;
                label1.Text = "Enter Password";
                textBox1.Enabled = true;
                textBox1.Focus();
                button1.Enabled = true;
            }
        }

        private void FadeIn(Form frm)
        {
            for (int i = 0; i <= 10; i++)
            {
                frm.Opacity = (float)i / 10;
                Thread.Sleep(1);
            }
        }

        private void FadeOut(Form frm)
        {
            for (int i = 10; i > 0; i--)
            {
                frm.Opacity = (float)i / 10;
                Thread.Sleep(1);
            }
            frm.Opacity = 0;
        }

        private void SpawnSecondaryForms()
        {
            Screen[] screens = Screen.AllScreens;
            int FormNum = 1;
            foreach (Screen s in screens)
            {
                // multiple monitors
                // if were inside the bounds of this screen skip spawning another form
                if (!s.Bounds.Contains(this.Location))
                {
                    Array.Resize(ref SecondaryForms, FormNum);
                    SecondaryForms[SecondaryForms.Length - 1] = new Form
                    {
                        Left = s.Bounds.Width,
                        Top = s.Bounds.Height,
                        StartPosition = FormStartPosition.Manual,
                        Location = new Point(s.Bounds.Location.X, s.Bounds.Location.Y),
                        FormBorderStyle = FormBorderStyle.None,
                        WindowState = FormWindowState.Maximized,
                        ShowInTaskbar = false,
                        TransparencyKey = Color.Green,
                        BackColor = Color.Green,
                        TopMost = true,
                        Text = Text + " Secondary Screen " + (FormNum - 1),
                        Opacity = 0
                    };
                    SecondaryForms[FormNum - 1].Show();
                    SecondaryForms[FormNum - 1].Activate();
                    FormNum++;
                }
            }
        }

        private void Lockdown()
        {
            Regedits.NoLogoff(Lockit.Conf.RemoveSignOut);
            Regedits.NoSwitchUser(Lockit.Conf.RemoveSwitchUser);
            Regedits.NoPowerMenu(Lockit.Conf.RemovePowerMenu);

            textBox1.Text = "";
            textBox1.Focus();
            label1.Text = "Enter Password";
            checkBox1.Checked = false;
            Deactivate += new EventHandler(Form1_Deactivate);
            Pinvoke.EnumWindows(new Pinvoke.WindowEnumCallback(MinimizeWindow), 0);
            KbdIntercept.InstallHook();
            SpawnSecondaryForms();
            System.Threading.Tasks.Task.Run(() => StayOnTopThread());

            switch (Lockit.Conf.Backtype)
            {
                case "Colour":
                    BackColor = Color.FromArgb(int.Parse(Lockit.Conf.BackColour));
                    TransparencyKey = Color.Empty;
                    for (int i = 0; i < SecondaryForms.Length; i++)
                    {
                        SecondaryForms[i].Opacity = 0;
                        SecondaryForms[i].BackColor = BackColor;
                        SecondaryForms[i].TransparencyKey = Color.Empty;
                        SecondaryForms[i].Update();
                        SecondaryForms[i].Activate();
                        FadeIn(SecondaryForms[i]);
                        SecondaryForms[i].TopMost = true;
                    }
                    break;
                case "Image":
                    if (File.Exists(Lockit.Conf.BackImage))
                    {
                        BackgroundImage = Image.FromFile(Lockit.Conf.BackImage);
                        for (int i = 0; i < SecondaryForms.Length; i++)
                        {
                            SecondaryForms[i].Opacity = 0;
                            SecondaryForms[i].BackgroundImage = BackgroundImage;
                            SecondaryForms[i].BackgroundImageLayout = BackgroundImageLayout; // should be zoom as set by designer
                            SecondaryForms[i].Activate();
                            FadeIn(SecondaryForms[i]);
                            SecondaryForms[i].TopMost = true;
                        }
                    }
                    if (Lockit.Conf.ImageBackType == "Transparent")
                    {
                        BackColor = Color.Red;
                        // this will cause issue with some png transparency
                        // but there's no other way to block clickthrough
                        TransparencyKey = Color.Red;
                        for (int i = 0; i < SecondaryForms.Length; i++)
                        {
                            SecondaryForms[i].Opacity = 0;
                            SecondaryForms[i].BackColor = BackColor;
                            SecondaryForms[i].TransparencyKey = TransparencyKey;
                            SecondaryForms[i].Update();
                            SecondaryForms[i].Activate();
                            FadeIn(SecondaryForms[i]);
                            SecondaryForms[i].TopMost = true;
                        }
                    }
                    else
                    {
                        if (Lockit.Conf.ImageBackColour != "")
                        {
                            BackColor = Color.FromArgb(int.Parse(Lockit.Conf.ImageBackColour));
                            TransparencyKey = Color.Empty;
                            for (int i = 0; i < SecondaryForms.Length; i++)
                            {
                                SecondaryForms[i].Opacity = 0;
                                SecondaryForms[i].BackColor = BackColor;
                                SecondaryForms[i].TransparencyKey = TransparencyKey;
                                SecondaryForms[i].Update();
                                SecondaryForms[i].Activate();
                                FadeIn(SecondaryForms[i]);
                                SecondaryForms[i].TopMost = true;
                            }
                        }
                    }
                    break;
                default:
                    BackColor = Color.Red; // red is the magic colour
                    TransparencyKey = Color.Red; // that doesn't allow clickthrough 
                    for (int i = 0; i < SecondaryForms.Length; i++)
                    {
                        SecondaryForms[i].Opacity = 0;
                        SecondaryForms[i].TransparencyKey = TransparencyKey;
                        SecondaryForms[i].BackColor = BackColor;
                        SecondaryForms[i].Update();
                        SecondaryForms[i].Activate();
                        FadeIn(SecondaryForms[i]);
                        SecondaryForms[i].TopMost = true;
                    }
                    break;

            }
            Update();
            
        }

        private async System.Threading.Tasks.Task LockOut(int delay)
        {
            for (int i = delay; i > 0; i--)
            {
                TimeSpan time = TimeSpan.FromSeconds(i);
                textBox1.Text = time.ToString(@"hh\:mm\:ss");
                await System.Threading.Tasks.Task.Delay(1000);
            }
        }

        private void TextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                OKButton_Click(sender, e);
            }
        }

        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            textBox1.UseSystemPasswordChar = !checkBox1.Checked;
            textBox1.Focus();
        }

        private void Form1_Deactivate(object sender, EventArgs e)
        {
            Pinvoke.EnumWindows(new Pinvoke.WindowEnumCallback(MinimizeWindow), 0); // whack a mole
        }

        public static bool IsWindowTopMost(IntPtr Handle)
        {
            return ((long)Pinvoke.GetWindowLongPtr(Handle, GWL_EXSTYLE) & WS_EX_TOPMOST) != 0;
        }

        private bool MinimizeWindow(int hwnd, int lparam) // AKA whack a mole
        {
            if (hwnd == (int)Handle)
                return false; // ignore our own form

            for (int i = 0; i < SecondaryForms.Length; i++)
            {
                if (hwnd == (int)SecondaryForms[i].Handle)
                    return false; // ignore our secondary forms
            }

            if (hwnd == (int)Pinvoke.FindWindow("Shell_TrayWnd", null))
                return false; // ignore taskbar

            // using FindWindow() as above fails us here if theres more
            // than one secondary taskbar
            var className = new StringBuilder(256);
            Pinvoke.GetClassName((IntPtr)hwnd, className, className.Capacity);
            if (className.ToString() == "Shell_SecondaryTrayWnd")
                return false; // ignore secondary taskbars

            // this works but, for admin progs eg. taskmanager with always on top
            // set, we need to be running as admin
            if (Pinvoke.IsWindowVisible(hwnd) && IsWindowTopMost((IntPtr)hwnd))
            {
                Pinvoke.ShowWindowAsync((IntPtr)hwnd, 11); // only minimize if it's topmost and visible
                return false;
            }

            return true;
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            if (CanClose)
                Close();
            Update();
            FadeIn(this);
        }

        delegate void DelegateStayOnTop();
        private readonly DelegateStayOnTop dStayOnTop;

        private void StayOnTopThread()
        {
            while (true)
            {
                StayOnTop();
                Thread.Sleep(1); // keeps our cpu usage down
            }
        }

        private void StayOnTop()
        {
            if (InvokeRequired)
            {
                try
                {
                    Invoke(dStayOnTop, new object[] { });
                }
                catch (ObjectDisposedException)
                {
                    Application.Exit();
                }
            }
            else
            {
                try
                {
                    TopMost = true;
                    Activate();
                }
                catch { }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!Lockit.Conf.KeepRemoveSignout)
                Regedits.NoLogoff(false);
            if (!Lockit.Conf.KeepRemoveSwitchUser)
                Regedits.NoSwitchUser(false);
            if (!Lockit.Conf.KeepRemovePowerMenu)
                Regedits.NoPowerMenu(false);

            KbdIntercept.RemoveHook();
        }
    }
}