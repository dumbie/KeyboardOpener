using System;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Win8_KeyboardOpener
{
    class DetectKeyboard
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetCurrentThreadId();

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern IntPtr GetFocus();

        [DllImport("user32.dll")]
        static extern bool GetClassName(IntPtr hWnd, StringBuilder ClassName, int ClassMax);

        [DllImport("user32.dll")]
        static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, IntPtr ThreadId);

        [DllImport("user32.dll")]
        static extern IntPtr AttachThreadInput(IntPtr AttachId, IntPtr AttachToId, bool AttachStatus);

        [DllImport("user32.dll")]
        static extern IntPtr FindWindow(string ClassName, string WindowName);

        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        //Application Variables
        Process Process = new Process();
        StringBuilder StringBuilder = new StringBuilder(50);
        static public int ActiveProcessFocusId = 0;
        static public string ActiveProcessFocus = "";
        static public string PreviousProcessFocus = "";

        //Detect Keyboard Requests
        public void DetectKeyBoard()
        {
            while (true)
            {
                try
                {
                    AttachThreadInput(GetWindowThreadProcessId(GetForegroundWindow(), IntPtr.Zero), GetCurrentThreadId(), true);
                    IntPtr GetFocusId = GetFocus();
                    AttachThreadInput(GetWindowThreadProcessId(GetForegroundWindow(), IntPtr.Zero), GetCurrentThreadId(), false);
                    if (GetClassName(GetFocusId, StringBuilder, StringBuilder.Capacity))
                    {
                        ActiveProcessFocus = StringBuilder.ToString();
                        if (PreviousProcessFocus != ActiveProcessFocus && !ActiveProcessFocus.Contains("KeyboardOpener"))
                        {
                            if (ActiveProcessFocus == "Edit" || ActiveProcessFocus == "SearchPane" || ActiveProcessFocus.Contains("RichEdit") || ActiveProcessFocus.Contains("SearchEdit") || ActiveProcessFocus.Contains("TextfieldEdit") || ActiveProcessFocus.Contains("Afx:00400000") || ActiveProcessFocus == "_WwG" || ActiveProcessFocus == "Scintilla" || ActiveProcessFocus == "SPEAD0C4")
                            {
                                try
                                {
                                    //Open TabTip / On Screen Keyboard
                                    if (ConfigurationManager.AppSettings["KeyboardType"] == "0") { Process.Start("C:\\Program Files\\Common Files\\Microsoft Shared\\ink\\TabTip.exe"); }
                                    else { Process.Start("C:\\Windows\\System32\\osk.exe"); }
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
                                }
                                catch { }
                            }
                            PreviousProcessFocus = ActiveProcessFocus;
                        }
                    }
                    Thread.Sleep(250);
                }
                catch { }
            }
        }
    }
}