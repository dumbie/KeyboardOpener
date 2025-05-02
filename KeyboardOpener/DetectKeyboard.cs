using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Automation;
using static Win8_KeyboardOpener.ManageKeyboard;

namespace Win8_KeyboardOpener
{
    class DetectKeyboard
    {
        //Dll Imports
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

        //Application Variables
        static public string vActiveProcessFocus = "";
        static public string vPreviousProcessFocus = "";

        //Detect Keyboard Requests
        public void DetectKeyBoard()
        {
            while (true)
            {
                try
                {
                    //bool openKeyboard = DetectTextbox_ClassName();
                    bool openKeyboard = DetectTextbox_UIAutomation();

                    //Fix manually opened keyboard closing
                    if (vPreviousProcessFocus != vActiveProcessFocus)
                    {
                        continue;
                    }

                    if (openKeyboard)
                    {
                        OpenKeyboard();
                    }
                    else
                    {
                        CloseKeyboard();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Failed detecting keyboard request: " + ex.Message);
                }
                finally
                {
                    Thread.Sleep(500);
                }
            }
        }

        private static bool DetectTextbox_ClassName()
        {
            try
            {
                bool openKeyboard = false;
                StringBuilder stringBuilder = new StringBuilder(50);

                AttachThreadInput(GetWindowThreadProcessId(GetForegroundWindow(), IntPtr.Zero), GetCurrentThreadId(), true);
                IntPtr focusedHandle = GetFocus();
                AttachThreadInput(GetWindowThreadProcessId(GetForegroundWindow(), IntPtr.Zero), GetCurrentThreadId(), false);

                if (GetClassName(focusedHandle, stringBuilder, stringBuilder.Capacity))
                {
                    vActiveProcessFocus = stringBuilder.ToString();
                    if (vPreviousProcessFocus != vActiveProcessFocus && !vActiveProcessFocus.Contains("KeyboardOpener"))
                    {
                        if (vActiveProcessFocus == "Edit" || vActiveProcessFocus == "SearchPane" || vActiveProcessFocus.Contains("RichEdit") || vActiveProcessFocus.Contains("SearchEdit") || vActiveProcessFocus.Contains("TextfieldEdit") || vActiveProcessFocus.Contains("Afx:00400000") || vActiveProcessFocus == "_WwG" || vActiveProcessFocus == "Scintilla" || vActiveProcessFocus == "SPEAD0C4")
                        {
                            openKeyboard = true;
                        }
                    }
                }

                return openKeyboard;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to detect textbox: " + ex.Message);
                return false;
            }
        }

        private static bool DetectTextbox_UIAutomation()
        {
            try
            {
                bool openKeyboard = false;

                IntPtr focusedHandle = GetFocus();
                if (focusedHandle == IntPtr.Zero)
                {
                    focusedHandle = GetForegroundWindow();
                }

                AutomationElement autoElement = AutomationElement.FromHandle(focusedHandle);
                //Fix FindAll seems to freeze for couple seconds on certain processes
                AutomationElementCollection controlEdit = autoElement.FindAll(TreeScope.Subtree, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit));
                AutomationElementCollection controlPane = autoElement.FindAll(TreeScope.Subtree, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Pane));
                AutomationElementCollection controlDocument = autoElement.FindAll(TreeScope.Subtree, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Document));
                AutomationElementCollection controlComboBox = autoElement.FindAll(TreeScope.Subtree, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.ComboBox));

                bool focusedEdit = controlEdit.Cast<AutomationElement>().ToList().Any(x => x.Current.HasKeyboardFocus);
                if (focusedEdit) { openKeyboard = true; }

                bool focusedPane = controlPane.Cast<AutomationElement>().ToList().Where(x => x.Current.ClassName.Contains("Scintilla") && x.Current.HasKeyboardFocus).Any();
                if (focusedPane) { openKeyboard = true; }

                bool focusedDocument = controlDocument.Cast<AutomationElement>().ToList().Where(x => x.Current.ClassName.Contains("Edit") && x.Current.HasKeyboardFocus).Any();
                if (focusedDocument) { openKeyboard = true; }

                bool focusedComboBox = controlComboBox.Cast<AutomationElement>().ToList().Any(x => x.Current.HasKeyboardFocus);
                if (focusedComboBox) { openKeyboard = true; }

                Debug.WriteLine("Edit controls: " + controlEdit.Count + " / " + focusedEdit);
                Debug.WriteLine("Pane controls: " + controlPane.Count + " / " + focusedPane);
                Debug.WriteLine("Document controls: " + controlDocument.Count + " / " + focusedDocument);
                Debug.WriteLine("ComboBox controls: " + controlComboBox.Count + " / " + focusedComboBox);

                ////Show debug information
                //var combined = controlEdit.Cast<AutomationElement>().ToList().Concat(controlDocument.Cast<AutomationElement>().ToList()).Concat(controlComboBox.Cast<AutomationElement>().ToList());
                //if (combined.Count() > 0)
                //{
                //    foreach (AutomationElement element in combined)
                //    {
                //        if (element != null && element.Current.HasKeyboardFocus)
                //        {
                //            Debug.WriteLine("Name: " + element.Current.Name);
                //            Debug.WriteLine("ItemType: " + element.Current.ItemType);
                //            Debug.WriteLine("ClassName: " + element.Current.ClassName);
                //            Debug.WriteLine("HasKeyboardFocus: " + element.Current.HasKeyboardFocus);
                //            Debug.WriteLine("ProgrammaticName: " + element.Current.ControlType.ProgrammaticName);
                //        }
                //    }
                //}

                return openKeyboard;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to detect textbox: " + ex.Message);
                return false;
            }
        }
    }
}