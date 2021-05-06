using System;
using System.Windows.Forms;
using DevExpress.UserSkins;
using DevExpress.Skins;
using DevExpress.LookAndFeel;
using Templates;

namespace DatusArator.Templates {
  static class Program {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main() {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);

      BonusSkins.Register();
      SkinManager.EnableFormSkins();
      UserLookAndFeel.Default.SetSkinStyle("DevExpress Style");
      Application.Run(new dlgEditGeneric());
    }
  }
}
