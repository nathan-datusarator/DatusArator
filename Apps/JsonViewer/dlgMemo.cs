using System;
using System.Windows.Forms;

namespace DatusArator.Windows.Dialogs {
  public partial class dlgMemo : Form {
    public static void ShowTextInMemo(string caption, string memo) {
      using (var form = new dlgMemo { Text = caption }) {
        form.memMain.Text = memo;
        form.memMain.ReadOnly = true;

        form.btnOk.Visible = false;

        form.ShowDialog();
      }
    }

    public static string EditTextInMemo(string caption, string memo) {
      using (var form = new dlgMemo { Text = caption }) {
        form.memMain.Text = memo;
        form.memMain.Select();

        form.btnClose.Text = "Cancel";

        var result = form.ShowDialog();

        return (result == DialogResult.OK) ? form.memMain.Text : memo;
      }
    }

    public dlgMemo() {
      InitializeComponent();
    }
  }
}
