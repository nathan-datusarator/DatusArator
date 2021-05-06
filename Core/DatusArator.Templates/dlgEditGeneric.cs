namespace Templates {
  public partial class dlgEditGeneric : DevExpress.XtraEditors.XtraForm {
    public static object Add() {

      return null;
    }

    public static bool Edit(object value) {
      using (var dlg = new dlgEditGeneric()) {
        dlg.Init(value);

        var result = dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK;
        if (result)
          dlg.SaveData(value);

        return result;
      }
    }

    public dlgEditGeneric() {
      InitializeComponent();
    }

    public void Init(object _) {

    }

    public void SaveData(object _) {
    }

    private void btnOK_Click(object sender, System.EventArgs e) {
      // Validation goes here

      DialogResult = System.Windows.Forms.DialogResult.OK;
    }
  }
}