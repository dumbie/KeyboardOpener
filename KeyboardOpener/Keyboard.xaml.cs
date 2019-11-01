using Microsoft.Win32;
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace Win8_KeyboardOpener
{
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        static extern IntPtr SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        static extern IntPtr FindWindow(string ClassName, string WindowName);

        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        //Application Variables
        Process Process = new Process();
        Thread ThreadDetectKeyboard = null;

        public MainWindow()
        {
            StartupChecks();
            InitializeComponent();
            Loaded += (sender, args) =>
            {
                new TrayMenu();
                DetectKeyboardLocation();
                StartDetectKeyboard();
            };
        }

        //Startup checks
        void StartupChecks()
        {
            try
            {
                if (Process.GetProcessesByName("KeyboardOpener").Length > 1)
                {
                    MessageBox.Show("Keyboard Opener is already running.", "Keyboard Opener");
                    Environment.Exit(1);
                }

                string ApplicationRootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase).Replace("file:\\", "").ToString();
                if (!File.Exists(ApplicationRootPath + "\\Assets\\icon_Keyboard.png"))
                {
                    MessageBox.Show("File: Assets\\icon_Keyboard.png could not be found.", "Keyboard Opener");
                    Environment.Exit(1);
                }

                if (!File.Exists(ApplicationRootPath + "\\Assets\\icon_KeyboardDisabled.png"))
                {
                    MessageBox.Show("File: Assets\\icon_KeyboardDisabled.png could not be found.", "Keyboard Opener");
                    Environment.Exit(1);
                }

                if (!File.Exists(ApplicationRootPath + "\\KeyboardOpener.exe"))
                {
                    MessageBox.Show("File: KeyboardOpener.exe could not be found.", "Keyboard Opener");
                    Environment.Exit(1);
                }

                if (!File.Exists(ApplicationRootPath + "\\KeyboardOpener.exe.config"))
                {
                    MessageBox.Show("File: KeyboardOpener.exe.config could not be found.", "Keyboard Opener");
                    Environment.Exit(1);
                }
            }
            catch { }
        }

        //Detect keyboard button location
        void DetectKeyboardLocation()
        {
            try
            {
                if (ConfigurationManager.AppSettings["KeyboardDisplayButton"] == "False")
                {
                    this.Visibility = Visibility.Collapsed;
                    this.IsEnabled = false;
                }
                else
                {
                    //Set keyboard button size
                    string KeyboardSize = ConfigurationManager.AppSettings["KeyboardSize"];
                    if (KeyboardSize == "0")
                    {
                        this.Width = 85;
                        this.MaxWidth = 85;
                        this.Height = 45;
                        this.MaxHeight = 45;
                    }
                    else if (KeyboardSize == "1")
                    {
                        this.Width = 127.5;
                        this.MaxWidth = 127.5;
                        this.Height = 67.5;
                        this.MaxHeight = 67.5;
                    }
                    else if (KeyboardSize == "2")
                    {
                        this.Width = 170;
                        this.MaxWidth = 170;
                        this.Height = 90;
                        this.MaxHeight = 90;
                    }

                    SetKeyboardLocation();
                    //Change keyboard location on window docking
                    SystemEvents.UserPreferenceChanged += (ks, ka) => { SetKeyboardLocation(); };
                }
            }
            catch { }
        }

        //Set keyboard button location
        void SetKeyboardLocation()
        {
            try
            {
                var WorkArea = SystemParameters.WorkArea;
                string KeyboardLocation = ConfigurationManager.AppSettings["KeyboardLocation"];

                //Set Keyboard button in bottom right
                if (KeyboardLocation == "0")
                {
                    Left = WorkArea.Width - this.Width;
                    Top = WorkArea.Height - this.Height;
                }
                //Set Keyboard button in bottom left
                else if (KeyboardLocation == "1")
                {
                    Left = (WorkArea.Width - this.Width) - WorkArea.Width + this.Width;
                    Top = WorkArea.Height - this.Height;
                }
                //Set Keyboard button in top right
                else if (KeyboardLocation == "2")
                {
                    Left = WorkArea.Width - this.Width;
                    Top = (WorkArea.Height - this.Height) - WorkArea.Height + this.Height;
                }
                //Set Keyboard button in top left
                else if (KeyboardLocation == "3")
                {
                    Left = (WorkArea.Width - this.Width) - WorkArea.Width + this.Width;
                    Top = (WorkArea.Height - this.Height) - WorkArea.Height + this.Height;
                }
            }
            catch { }
        }

        //Start keyboard automatic thread
        void StartDetectKeyboard()
        {
            try
            {
                if (ConfigurationManager.AppSettings["KeyboardAutomatic"] == "True")
                {
                    ThreadDetectKeyboard = new Thread(new DetectKeyboard().DetectKeyBoard);
                    ThreadDetectKeyboard.Start();
                }
            }
            catch { }
        }

        //Open/close Keyboard button handle
        void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                this.Opacity = 1;
                if ((GetWindowLong(FindWindow("IPTip_Main_Window", null), -16) & 0x8000000) != 0 || Process.GetProcessesByName("osk").Length == 0 && Process.GetProcessesByName("TabTip").Length == 0)
                {
                    try
                    {
                        //Open TabTip / On Screen Keyboard
                        if (ConfigurationManager.AppSettings["KeyboardType"] == "0") { Process.Start("C:\\Program Files\\Common Files\\Microsoft Shared\\ink\\TabTip.exe"); }
                        else { Process.Start("C:\\Windows\\System32\\osk.exe"); }

                        //Focus back on active window
                        Process FocusProcess = Process.GetProcessById(DetectKeyboard.ActiveProcessFocusId);
                        if (FocusProcess.ProcessName != "explorer") { SetForegroundWindow(FocusProcess.MainWindowHandle); }
                    }
                    catch { }
                }
                else
                {
                    try
                    {
                        //Close TabTip / On Screen Keyboard
                        SendMessage(FindWindow("IPTip_Main_Window", null), 0x0112, (IntPtr)0xF060, IntPtr.Zero);
                        foreach (Process KillProc in Process.GetProcessesByName("osk")) { KillProc.Kill(); }

                        //Focus back on active window
                        Process FocusProcess = Process.GetProcessById(DetectKeyboard.ActiveProcessFocusId);
                        if (FocusProcess.ProcessName != "explorer") { SetForegroundWindow(FocusProcess.MainWindowHandle); }
                    }
                    catch { }
                }
            }
            catch { }
        }

        //Keyboard button press effect
        void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        { this.Opacity = 0.75; }

        //Enable/disabled automatic keyboard double click
        void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (ConfigurationManager.AppSettings["KeyboardAutomatic"] == "True")
                {
                    if (ThreadDetectKeyboard.IsAlive)
                    {
                        img_KeyboardEnabled.Visibility = Visibility.Collapsed;
                        img_KeyboardDisabled.Visibility = Visibility.Visible;
                        ThreadDetectKeyboard.Abort();
                    }
                    else
                    {
                        img_KeyboardEnabled.Visibility = Visibility.Visible;
                        img_KeyboardDisabled.Visibility = Visibility.Collapsed;
                        ThreadDetectKeyboard = new Thread(new DetectKeyboard().DetectKeyBoard);
                        ThreadDetectKeyboard.Start();
                    }
                }
            }
            catch { }
        }

        //Open settings window right mouse click
        void Window_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        { new SettingsWindow().Show(); }
    }
}