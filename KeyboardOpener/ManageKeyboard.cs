using System;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using static Win8_KeyboardOpener.AppImport;
using static Win8_KeyboardOpener.AppVariables;

namespace Win8_KeyboardOpener
{
    class ManageKeyboard
    {
        public static bool IsKeyboardOpen()
        {
            try
            {
                if (ConfigurationManager.AppSettings["KeyboardType"] == "0")
                {
                    //Check TabTip
                    Type targetType = Type.GetTypeFromCLSID(Guid.Parse("D5120AA3-46BA-44C5-822D-CA8092C1FC72"));
                    IFrameworkInputPane frameworkInputPane = (IFrameworkInputPane)Activator.CreateInstance(targetType);
                    frameworkInputPane.Location(out Rectangle rect);
                    Marshal.ReleaseComObject(frameworkInputPane);
                    return !rect.IsEmpty;
                }
                else
                {
                    //Check On Screen Keyboard
                    return Process.GetProcessesByName("osk").Any();
                }
            }
            catch
            {
                return false;
            }
        }

        public static void CloseKeyboard(bool manual)
        {
            try
            {
                //Set manual keyboard status
                if (manual)
                {
                    vKeyboardManual = false;
                }

                //Check if keyboard is already closed
                if (!IsKeyboardOpen())
                {
                    Debug.WriteLine("Keyboard is already closed, skipping.");
                    return;
                }

                //Check keyboard type
                if (ConfigurationManager.AppSettings["KeyboardType"] == "0")
                {
                    Debug.WriteLine("Closing keyboard TabTip " + DateTime.Now);

                    //Close TabTip
                    if (IsKeyboardOpen())
                    {
                        Type targetType = Type.GetTypeFromCLSID(Guid.Parse("4CE576FA-83DC-4F88-951C-9D0782B4E376"));
                        ITipInvocation tipInvocation = (ITipInvocation)Activator.CreateInstance(targetType);
                        tipInvocation.Toggle(GetDesktopWindow());
                        Marshal.ReleaseComObject(tipInvocation);
                    }
                }
                else
                {
                    Debug.WriteLine("Closing keyboard osk " + DateTime.Now);

                    //Close On Screen Keyboard
                    foreach (Process processKill in Process.GetProcessesByName("osk"))
                    {
                        processKill.Kill();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to close keyboard: " + ex.Message);
            }
        }

        public static void OpenKeyboard(bool manual)
        {
            try
            {
                //Set manual keyboard status
                if (manual)
                {
                    vKeyboardManual = true;
                }

                //Check if keyboard is already open
                if (IsKeyboardOpen())
                {
                    Debug.WriteLine("Keyboard is already open, skipping.");
                    return;
                }

                //Check keyboard type
                if (ConfigurationManager.AppSettings["KeyboardType"] == "0")
                {
                    //Open TabTip
                    Debug.WriteLine("Opening keyboard TabTip " + DateTime.Now);

                    if (!IsKeyboardOpen())
                    {
                        string rootPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles);
                        Process process = new Process();
                        process.StartInfo.FileName = Path.Combine(rootPath, "Microsoft Shared\\ink\\TabTip.exe");
                        process.StartInfo.Arguments = "/ManualLaunch";
                        process.Start();

                        Type targetType = Type.GetTypeFromCLSID(Guid.Parse("4CE576FA-83DC-4F88-951C-9D0782B4E376"));
                        ITipInvocation tipInvocation = (ITipInvocation)Activator.CreateInstance(targetType);
                        tipInvocation.Toggle(GetDesktopWindow());
                        Marshal.ReleaseComObject(tipInvocation);
                    }
                }
                else
                {
                    //Open On Screen Keyboard
                    Debug.WriteLine("Opening keyboard osk " + DateTime.Now);

                    string rootPath = Environment.GetFolderPath(Environment.SpecialFolder.System);
                    Process process = new Process();
                    process.StartInfo.FileName = Path.Combine(rootPath, "osk.exe");
                    process.Start();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to open keyboard: " + ex.Message);
            }
        }
    }
}