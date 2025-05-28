using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace Win8_KeyboardOpener
{
    class AppImport
    {
        //Dll Imports
        [DllImport("kernel32.dll")]
        public static extern IntPtr GetCurrentThreadId();

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern IntPtr GetFocus();

        [DllImport("user32.dll")]
        public static extern bool GetClassName(IntPtr hWnd, StringBuilder ClassName, int ClassMax);

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, IntPtr ThreadId);

        [DllImport("user32.dll")]
        public static extern IntPtr AttachThreadInput(IntPtr AttachId, IntPtr AttachToId, bool AttachStatus);

        [DllImport("user32.dll")]
        public static extern IntPtr SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string ClassName, string WindowName);

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();

        [ComImport, Guid("37c994e7-432b-4834-a2f7-dce1f13b834b"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface ITipInvocation
        {
            int Toggle(IntPtr hWnd);
        }

        [ComImport, Guid("5752238b-24f0-495a-82f1-2fd593056796"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IFrameworkInputPane
        {
            int Advise([MarshalAs(UnmanagedType.IUnknown)] object pWindow, [MarshalAs(UnmanagedType.IUnknown)] object pHandler, out int pdwCookie);
            int AdviseWithHWND(IntPtr hwnd, [MarshalAs(UnmanagedType.IUnknown)] object pHandler, out int pdwCookie);
            int Unadvise(int pdwCookie);
            int Location(out Rectangle prcInputPaneScreenLocation);
        }
    }
}