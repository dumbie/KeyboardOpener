using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using static Win8_KeyboardOpener.ManageKeyboard;

namespace Win8_KeyboardOpener
{
    class AppStartup
    {
        //Startup checks
        public static void AppStartupChecks()
        {
            try
            {
                if (Process.GetProcessesByName("KeyboardOpener").Length > 1)
                {
                    MessageBox.Show("Keyboard Opener is already running.", "Keyboard Opener");
                    Environment.Exit(0);
                }

                string ApplicationRootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase).Replace("file:\\", "").ToString();
                if (!File.Exists(ApplicationRootPath + "\\Assets\\icon_Keyboard.png"))
                {
                    MessageBox.Show("File: Assets\\icon_Keyboard.png could not be found.", "Keyboard Opener");
                    Environment.Exit(0);
                }

                if (!File.Exists(ApplicationRootPath + "\\Assets\\icon_KeyboardDisabled.png"))
                {
                    MessageBox.Show("File: Assets\\icon_KeyboardDisabled.png could not be found.", "Keyboard Opener");
                    Environment.Exit(0);
                }

                if (!File.Exists(ApplicationRootPath + "\\KeyboardOpener.exe"))
                {
                    MessageBox.Show("File: KeyboardOpener.exe could not be found.", "Keyboard Opener");
                    Environment.Exit(0);
                }

                if (!File.Exists(ApplicationRootPath + "\\KeyboardOpener.exe.config"))
                {
                    MessageBox.Show("File: KeyboardOpener.exe.config could not be found.", "Keyboard Opener");
                    Environment.Exit(0);
                }
            }
            catch { }
        }

        //Exit application
        public static void AppExit()
        {
            try
            {
                //Close the keyboard
                CloseKeyboard(false);

                //Hide tray icon
                TrayMenu.NotifyIcon.Visible = false;

                //Exit application
                Environment.Exit(0);
            }
            catch { }
        }
    }
}