using System;
using System.Net;
using System.Windows.Forms;

using Toolbox.AuthenticVacations;

namespace Toolbox {
  public partial class frmMain : Form {
    public readonly static string nl = Environment.NewLine;
    public frmMain() {
      InitializeComponent();
    }

    private void btnInterests_Click(object sender, EventArgs e) {
      txtResults.Text = AV_Tools.CompileInterests();
    }

    private void btnTourCompiler_Click(object sender, EventArgs e) {
      txtResults.Text = AV_Tools.CompileTours();
    }

    private void btnWorldMap_Click(object sender, EventArgs e) {
      txtResults.Text = AV_Tools.BuildWorldMap();
    }

    private void btnUSMap_Click(object sender, EventArgs e) {
      txtResults.Text = AV_Tools.BuildUSMap();
    }

    private void btnUK_Click(object sender, EventArgs e) {
      txtResults.Text = AV_Tools.BuildUKMap();
    }

    private void btnSpacingCSS_Click(object sender, EventArgs e) {
      txtResults.Text = "";
      for (int i = 1; i < 13; i++) {
        BuildMarginAndPadding(i + "", i);
      }

      for (int i = 0; i < 5; i++) {
        BuildMarginAndPadding(i + "-5", i + 0.5m);
      }

      void BuildMarginAndPadding(string css, decimal amount) {
        BuildElement(css, amount, "pl", "padding-left");
        BuildElement(css, amount, "pt", "padding-top");
        BuildElement(css, amount, "pr", "padding-right");
        BuildElement(css, amount, "pb", "padding-bottom");
        BuildElement(css, amount, "ml", "margin-left");
        BuildElement(css, amount, "mt", "margin-top");
        BuildElement(css, amount, "mr", "margin-right");
        BuildElement(css, amount, "mb", "margin-bottom");

        BuildElement(css, amount, "px", "padding-left", "padding-right");
        BuildElement(css, amount, "py", "padding-top", "padding-bottom");
        BuildElement(css, amount, "pa", "padding");

        BuildElement(css, amount, "mx", "margin-left", "margin-right");
        BuildElement(css, amount, "my", "margin-top", "margin-bottom");
        BuildElement(css, amount, "ma", "margin");

        txtResults.AppendText(nl);
      }

      void BuildElement(string css, decimal amount, string prefix, string element, string element2 = null) {
        var text = ".q-" + prefix + "-" + css + " { " + element + ": " + amount + "px; ";
        if (element2 != null)
          text += element2 + ": " + amount + "px; ";
        text += "}";

        txtResults.AppendText(text + nl);
      }
    }

    private void btnDestinations_Click(object sender, EventArgs e) {
      txtResults.Text = AV_Tools.MapDestinations(txtResults.Text);
    }

    private void btnOneOff_Click(object sender, EventArgs e) {
    }
  }
}
