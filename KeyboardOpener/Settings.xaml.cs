using Microsoft.Win32;
using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using static Win8_KeyboardOpener.AppStartup;

namespace Win8_KeyboardOpener
{
    public partial class SettingsWindow : Window
    {
        [DllImport("user32.dll")]
        static extern IntPtr FindWindow(string ClassName, string WindowName);

        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        public SettingsWindow()
        {
            InitializeComponent();
            SettingsLoad();
        }

        //Load - Application Settings
        void SettingsLoad()
        {
            try
            {
                txt_version.Text = "Keyboard Opener - v" + System.Reflection.Assembly.GetExecutingAssembly().FullName.Split('=')[1].Split(',')[0] + "\nBy Arnold Vink";
                cb_KeyboardType.SelectedIndex = Convert.ToInt32(ConfigurationManager.AppSettings["KeyboardType"]);
                cb_KeyboardLocation.SelectedIndex = Convert.ToInt32(ConfigurationManager.AppSettings["KeyboardLocation"]);
                cb_KeyboardSize.SelectedIndex = Convert.ToInt32(ConfigurationManager.AppSettings["KeyboardSize"]);
                cb_KeyboardAutomatic.IsChecked = Convert.ToBoolean(ConfigurationManager.AppSettings["KeyboardAutomatic"]);
                cb_KeyboardDisplayButton.IsChecked = Convert.ToBoolean(ConfigurationManager.AppSettings["KeyboardDisplayButton"]);

                //Check if application starts on Windows Startup
                RegistryKey StartupRegistryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                foreach (string StartupApp in StartupRegistryKey.GetValueNames())
                {
                    try
                    {
                        if (StartupApp == "Keyboard Opener")
                        {
                            cb_StartupWindows.IsChecked = true;
                            //Update to the current application directory
                            StartupRegistryKey.SetValue("Keyboard Opener", "\"" + Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase).Replace("file:\\", "").ToString() + "\\KeyboardOpener.exe" + "\"");
                        }
                    }
                    catch { }
                }
            }
            catch (Exception Ex) { MessageBox.Show("SettingsLoadError: " + Ex.Message, "Keyboard Opener"); }
            SettingsSaveEvents();
        }

        //Save Events - Application Settings
        void SettingsSaveEvents()
        {
            try
            {
                Configuration Configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                //Save - Keyboard open type
                cb_KeyboardType.SelectionChanged += (sender, e) =>
                {
                    if (Convert.ToInt32(ConfigurationManager.AppSettings["KeyboardType"]) != cb_KeyboardType.SelectedIndex)
                    {
                        Configuration.AppSettings.Settings["KeyboardType"].Value = cb_KeyboardType.SelectedIndex.ToString();
                        Configuration.Save();
                        ConfigurationManager.RefreshSection("appSettings");
                    }
                };

                //Save - Keyboard button location
                cb_KeyboardLocation.SelectionChanged += (sender, e) =>
                {
                    if (Convert.ToInt32(ConfigurationManager.AppSettings["KeyboardLocation"]) != cb_KeyboardLocation.SelectedIndex)
                    {
                        Configuration.AppSettings.Settings["KeyboardLocation"].Value = cb_KeyboardLocation.SelectedIndex.ToString();
                        Configuration.Save();
                        ConfigurationManager.RefreshSection("appSettings");
                    }
                };

                //Save - Keyboard button size
                cb_KeyboardSize.SelectionChanged += (sender, e) =>
                {
                    if (Convert.ToInt32(ConfigurationManager.AppSettings["KeyboardSize"]) != cb_KeyboardSize.SelectedIndex)
                    {
                        Configuration.AppSettings.Settings["KeyboardSize"].Value = cb_KeyboardSize.SelectedIndex.ToString();
                        Configuration.Save();
                        ConfigurationManager.RefreshSection("appSettings");
                    }
                };

                //Save - Automatically open the keyboard
                cb_KeyboardAutomatic.Click += (sender, e) =>
                {
                    if ((bool)cb_KeyboardAutomatic.IsChecked)
                    { Configuration.AppSettings.Settings["KeyboardAutomatic"].Value = "True"; }
                    else { Configuration.AppSettings.Settings["KeyboardAutomatic"].Value = "False"; }

                    Configuration.Save();
                    ConfigurationManager.RefreshSection("appSettings");
                };

                //Save - Display the keyboard open button
                cb_KeyboardDisplayButton.Click += (sender, e) =>
                {
                    if ((bool)cb_KeyboardDisplayButton.IsChecked)
                    { Configuration.AppSettings.Settings["KeyboardDisplayButton"].Value = "True"; }
                    else { Configuration.AppSettings.Settings["KeyboardDisplayButton"].Value = "False"; }

                    Configuration.Save();
                    ConfigurationManager.RefreshSection("appSettings");
                };

                //Save - Application Windows Startup
                cb_StartupWindows.Click += (sender, e) =>
                {
                    try
                    {
                        RegistryKey StartupRegistryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                        if ((bool)cb_StartupWindows.IsChecked) { StartupRegistryKey.SetValue("Keyboard Opener", "\"" + Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase).Replace("file:\\", "").ToString() + "\\KeyboardOpener.exe" + "\""); }
                        else { StartupRegistryKey.DeleteValue("Keyboard Opener", false); }
                    }
                    catch { }
                };
            }
            catch (Exception Ex) { MessageBox.Show("SettingsSaveError: " + Ex.Message, "Keyboard Opener"); }
        }

        //Check for application update
        async void btn_CheckVersion_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Download Current Version
                WebClient WebClient = new WebClient();
                WebClient.Headers[HttpRequestHeader.UserAgent] = "KeyboardOpener";
                string ResCurrentVersion = await WebClient.DownloadStringTaskAsync(new Uri("http://download.arnoldvink.com/KeyboardOpener.zip-version.txt" + "?nc=" + Environment.TickCount));
                if (ResCurrentVersion != System.Reflection.Assembly.GetExecutingAssembly().FullName.Split('=')[1].Split(',')[0])
                { MessageBox.Show("New version has been found: v" + ResCurrentVersion, "Keyboard Opener"); }
                else { MessageBox.Show("No new update has been found.", "Keyboard Opener"); }
            }
            catch { MessageBox.Show("Failed to check for the latest application version,\nplease check your internet connection and try again.", "Keyboard Opener"); }
        }

        //Exit the application
        void btn_ExitApp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AppExit();
            }
            catch { }
        }
    }
}