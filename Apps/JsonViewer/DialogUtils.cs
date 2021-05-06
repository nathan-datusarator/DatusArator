using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DatusArator.Windows.Dialogs {
  public static class DialogUtils {
    public static string nl = Environment.NewLine;

    public static void ShowMessage(string message, string title = null, MessageBoxIcon icon = MessageBoxIcon.None) {
      MessageBox.Show(message, title ?? "Alert", MessageBoxButtons.OK, icon);
    }

    public static bool AskQuestion(string question, string title = null, MessageBoxIcon icon = MessageBoxIcon.None) {
      DialogResult result = MessageBox.Show(question, title ?? "Confirm", MessageBoxButtons.YesNo, icon);
      return result == DialogResult.Yes;
    }

    public static string Prompt(string label, string caption, string defaultValue) {
      using (var prompt = new dlgPrompt {
        TopMost = true,
        Text = caption,
        Label = label,
        Value = defaultValue
      }) {
        if (prompt.ShowDialog() == DialogResult.OK)
          return prompt.Value;
        else
          return null;
      }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass")]
    [DllImport("User32.dll")]
    private static extern Int32 SetForegroundWindow(int hWnd);

    public static void BringWindowToFront(Form win) {
      SetForegroundWindow(win.Handle.ToInt32());
    }

    public static void ShowTextInMemo(string caption, string memo) {
      dlgMemo.ShowTextInMemo(caption, memo);
    }

    public static void ShowError(Exception ex) {
      ShowTextInMemo("Error: " + ex.Message, ex.Message + nl + nl + ex.StackTrace);
    }

    public static string EditTextInMemo(string caption, string value = "") {
      return dlgMemo.EditTextInMemo(caption, value);
    }

    public static string OpenFile(string fileName = "", string filters = "") {
      using (var dlg = new OpenFileDialog {
        FileName = fileName,
        Filter = filters
      }) {

        if (dlg.ShowDialog() == DialogResult.OK)
          return dlg.FileName;
        else
          return null;
      }
    }
  }
}
