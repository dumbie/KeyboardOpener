using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;
using static Win8_KeyboardOpener.AppImport;
using static Win8_KeyboardOpener.AppVariables;
using static Win8_KeyboardOpener.ManageKeyboard;

namespace Win8_KeyboardOpener
{
    class DetectKeyboard
    {
        //Detect Keyboard Requests
        public async void DetectKeyBoard()
        {
            while (true)
            {
                try
                {
                    //Check if keyboard is opened manually
                    if (vKeyboardManual)
                    {
                        await Task.Delay(500);
                        Debug.WriteLine("Keyboard is opened manually, skipping.");
                        if (!IsKeyboardOpen()) { vKeyboardManual = false; }
                        continue;
                    }

                    //Check if keyboard needs to be opened
                    //bool openKeyboard = DetectTextbox_ClassName();
                    bool openKeyboard = DetectTextbox_UIAutomation();

                    //Open or close keyboard
                    if (openKeyboard)
                    {
                        OpenKeyboard(false);
                    }
                    else
                    {
                        CloseKeyboard(false);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Failed detecting keyboard request: " + ex.Message);
                }
                finally
                {
                    await Task.Delay(500);
                }
            }
        }

        private static IntPtr GetFocusedHandle()
        {
            try
            {
                IntPtr currentThreadId = GetCurrentThreadId();
                IntPtr foregroundWindow = GetForegroundWindow();
                AttachThreadInput(GetWindowThreadProcessId(foregroundWindow, IntPtr.Zero), currentThreadId, true);
                IntPtr focusedHandle = GetFocus();
                AttachThreadInput(GetWindowThreadProcessId(foregroundWindow, IntPtr.Zero), currentThreadId, false);
                if (focusedHandle == IntPtr.Zero)
                {
                    return foregroundWindow;
                }
                else
                {
                    return focusedHandle;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to get focused handle: " + ex.Message);
                return IntPtr.Zero;
            }
        }

        private static bool DetectTextbox_ClassName()
        {
            try
            {
                bool openKeyboard = false;

                StringBuilder stringBuilder = new StringBuilder(256);
                if (GetClassName(GetFocusedHandle(), stringBuilder, stringBuilder.Capacity))
                {
                    string className = stringBuilder.ToString();
                    if (className == "Edit" || className == "SearchPane" || className.Contains("RichEdit") || className.Contains("SearchEdit") || className.Contains("TextfieldEdit") || className.Contains("Afx:") || className == "_WwG" || className == "Scintilla" || className == "SPEAD0C4")
                    {
                        openKeyboard = true;
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

                AutomationElement autoElement = AutomationElement.FromHandle(GetFocusedHandle());
                //Fix FindAll seems to freeze for couple seconds on certain processes like Steam
                AutomationElementCollection controlEdit = autoElement.FindAll(TreeScope.Subtree, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit));
                AutomationElementCollection controlPane = autoElement.FindAll(TreeScope.Subtree, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Pane));
                AutomationElementCollection controlDocument = autoElement.FindAll(TreeScope.Subtree, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Document));
                AutomationElementCollection controlComboBox = autoElement.FindAll(TreeScope.Subtree, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.ComboBox));

                bool focusedEdit = controlEdit.Cast<AutomationElement>().ToList().Any(x => x.Current.HasKeyboardFocus);
                if (focusedEdit) { openKeyboard = true; }

                bool focusedPane = controlPane.Cast<AutomationElement>().ToList().Where(x => (x.Current.ClassName.Contains("Scintilla") || x.Current.ClassName.Contains("Afx:")) && x.Current.HasKeyboardFocus).Any();
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