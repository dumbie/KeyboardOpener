using Microsoft.Win32;
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using static Win8_KeyboardOpener.AppImport;
using static Win8_KeyboardOpener.AppVariables;
using static Win8_KeyboardOpener.ManageKeyboard;
using static Win8_KeyboardOpener.AppStartup;

namespace Win8_KeyboardOpener
{
    public partial class MainWindow : Window
    {
        //Window Variables
        private IntPtr vInteropWindowHandle = IntPtr.Zero;

        //Window Initialize
        public MainWindow() { InitializeComponent(); }

        //Window Initialized
        protected override void OnSourceInitialized(EventArgs e)
        {
            try
            {
                //Get interop window handle
                vInteropWindowHandle = new WindowInteropHelper(this).EnsureHandle();

                AppStartupChecks();
                new TrayMenu();

                SetWindowStyle();
                SetKeyboardVisibility();
                SetKeyboardSize();
                SetKeyboardLocation();
                StartDetectKeyboard();

                //Change keyboard location on window docking
                SystemEvents.UserPreferenceChanged += (ks, ka) => { SetKeyboardLocation(); };
            }
            catch { }
        }

        //Set window style
        void SetWindowStyle()
        {
            try
            {
                int GWL_EXSTYLE = -20;
                int WS_EX_NOACTIVATE = 0x8000000;

                int exStyle = GetWindowLong(vInteropWindowHandle, GWL_EXSTYLE);
                exStyle |= WS_EX_NOACTIVATE;
                SetWindowLong(vInteropWindowHandle, GWL_EXSTYLE, exStyle);

                Debug.WriteLine("Window style set.");
            }
            catch { }
        }

        //Set keyboard button visibility
        void SetKeyboardVisibility()
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
                    this.Visibility = Visibility.Visible;
                    this.IsEnabled = true;
                }

                Debug.WriteLine("Keyboard visibility set.");
            }
            catch { }
        }

        //Set keyboard button size
        void SetKeyboardSize()
        {
            try
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

                Debug.WriteLine("Keyboard size set.");
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

                Debug.WriteLine("Keyboard location set.");
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
                    vThreadDetectKeyboard = new Thread(new DetectKeyboard().DetectKeyBoard);
                    vThreadDetectKeyboard.Start();
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
                if (!IsKeyboardOpen())
                {
                    OpenKeyboard(true);
                }
                else
                {
                    CloseKeyboard(true);
                }
            }
            catch { }
        }

        //Keyboard button press effect
        void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                this.Opacity = 0.75;
            }
            catch { }
        }

        //Enable/disabled automatic keyboard double click
        void Window_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (ConfigurationManager.AppSettings["KeyboardAutomatic"] == "True")
                {
                    if (vThreadDetectKeyboard.IsAlive)
                    {
                        img_KeyboardEnabled.Visibility = Visibility.Collapsed;
                        img_KeyboardDisabled.Visibility = Visibility.Visible;
                        vThreadDetectKeyboard.Abort();
                    }
                    else
                    {
                        img_KeyboardEnabled.Visibility = Visibility.Visible;
                        img_KeyboardDisabled.Visibility = Visibility.Collapsed;
                        vThreadDetectKeyboard = new Thread(new DetectKeyboard().DetectKeyBoard);
                        vThreadDetectKeyboard.Start();
                    }
                }
            }
            catch { }
        }
    }
}