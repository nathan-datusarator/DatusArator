using System.Windows.Forms;

namespace DatusArator.Windows.Dialogs {
  public partial class dlgPrompt : Form {
    public string Label { get { return lblText.Text; } set { lblText.Text = value; } }
    public string Value { get { return txtMain.Text; } set { txtMain.Text = value; } }

    public dlgPrompt() {
      InitializeComponent();

      txtMain.Focus();
    }
  }
}