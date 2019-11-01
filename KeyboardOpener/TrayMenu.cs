using System;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Win8_KeyboardOpener
{
    public partial class TrayMenu
    {
        [DllImport("user32.dll")]
        static extern IntPtr FindWindow(string ClassName, string WindowName);

        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        //Application Variables
        static public NotifyIcon NotifyIcon = new NotifyIcon();
        static public ContextMenu ContextMenu = new ContextMenu();

        public TrayMenu()
        {
            // Create a context menu for systray.
            ContextMenu.MenuItems.Add("Settings", OnSettings);
            ContextMenu.MenuItems.Add("Website", OnWebsite);
            ContextMenu.MenuItems.Add("Exit", OnExit);

            // Initialize the tray notify icon.
            NotifyIcon.Text = "Keyboard Opener";
            NotifyIcon.Icon = new Icon(Assembly.GetExecutingAssembly().GetManifestResourceStream("Win8_KeyboardOpener.Assets.icon_Application.ico"));

            // Handle Double Click event
            NotifyIcon.DoubleClick += new EventHandler(NotifyIcon_DoubleClick);

            // Add menu to tray icon and show it.
            NotifyIcon.ContextMenu = ContextMenu;
            NotifyIcon.Visible = true;
        }

        void NotifyIcon_DoubleClick(object Sender, EventArgs e)
        { new SettingsWindow().Show(); }

        void OnSettings(object sender, EventArgs e)
        { new SettingsWindow().Show(); }

        void OnWebsite(object sender, EventArgs e)
        { Process.Start("http://arnoldvink.com?p=projects"); }

        //Exit the application
        void OnExit(object sender, EventArgs e)
        {
            try
            {
                //Close TabTip / On Screen Keyboard
                SendMessage(FindWindow("IPTip_Main_Window", null), 0x0112, (IntPtr)0xF060, IntPtr.Zero);
                foreach (Process KillProc in Process.GetProcessesByName("osk")) { KillProc.Kill(); }

                NotifyIcon.Visible = false;
                Environment.Exit(1);
            }
            catch { }
        }
    }
}