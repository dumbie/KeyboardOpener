using System;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using static Win8_KeyboardOpener.AppStartup;

namespace Win8_KeyboardOpener
{
    public partial class TrayMenu
    {
        //Tray Variables
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
        {
            new SettingsWindow().Show();
        }

        void OnSettings(object sender, EventArgs e)
        {
            new SettingsWindow().Show();
        }

        void OnWebsite(object sender, EventArgs e)
        {
            Process.Start("https://projects.arnoldvink.com");
        }

        //Exit application
        void OnExit(object sender, EventArgs e)
        {
            try
            {
                AppExit();
            }
            catch { }
        }
    }
}