using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MyTrayApp
{
    public class SysTrayApp : Form
    {
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint ProcessId);
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        public static extern IntPtr SendMessageW(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        private const int APPCOMMAND_VOLUME_MUTE = 0x80000;
        private const int APPCOMMAND_VOLUME_UP = 0xA0000;
        private const int APPCOMMAND_VOLUME_DOWN = 0x90000;
        private const int WM_APPCOMMAND = 0x319;

        LowLevelKeyboardListener llkl;

        [STAThread]
        public static void Main()
        {
            Application.Run(new SysTrayApp());
        }

        private NotifyIcon trayIcon;
        private ContextMenu trayMenu;

        public SysTrayApp()
        {
            llkl = new LowLevelKeyboardListener();
            llkl.OnKeyPressed += Llkl_OnKeyPressed;
            llkl.HookKeyboard();

            // Create a simple tray menu with only one item.
            trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add("Exit", OnExit);

            // Create a tray icon. In this example we use a
            // standard system icon for simplicity, but you
            // can of course use your own custom icon too.
            trayIcon = new NotifyIcon();
            trayIcon.Text = "SimpleVolumeController";
            trayIcon.Icon = SoundController.Properties.Resources.slider;

            // Add menu to tray icon and show it.
            trayIcon.ContextMenu = trayMenu;
            trayIcon.Visible = true;
        }

        private void Llkl_OnKeyPressed(object sender, LowLevelKeyboardListener.KeyPressedArgs e)
        {
            if (e.KeyPressed == (int)Keys.Add)
            {
                IntPtr handle = GetForegroundWindow();
                uint pid;
                GetWindowThreadProcessId(handle, out pid);

                var vol = VolumeMixer.GetApplicationVolume((int)pid);
                if (vol.HasValue)
                {
                    VolumeMixer.SetApplicationVolume((int)pid, (float)vol + 2);
                }
            }
            if (e.KeyPressed == (int)Keys.Subtract)
            {
                IntPtr handle = GetForegroundWindow();
                uint pid;
                GetWindowThreadProcessId(handle, out pid);

                var vol = VolumeMixer.GetApplicationVolume((int)pid);
                if (vol.HasValue)
                {
                    VolumeMixer.SetApplicationVolume((int)pid, (float)vol - 2);
                }
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            Visible = false; // Hide form window.
            ShowInTaskbar = false; // Remove from taskbar.

            base.OnLoad(e);
        }

        private void OnExit(object sender, EventArgs e)
        {
            llkl.UnHookKeyboard();
            Application.Exit();
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                // Release the icon resource.
                trayIcon.Dispose();
            }

            base.Dispose(isDisposing);
        }
    }
}