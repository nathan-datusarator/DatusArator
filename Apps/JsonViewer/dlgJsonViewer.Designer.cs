namespace DatusArator.Windows.Dialogs {
  partial class dlgJsonViewer {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing) {
      if (disposing && (components != null)) {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent() {
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(dlgJsonViewer));
      this.pnlFooter = new System.Windows.Forms.Panel();
      this.btnNewJson = new System.Windows.Forms.Button();
      this.btnClose = new System.Windows.Forms.Button();
      this.pnlLeft = new System.Windows.Forms.Panel();
      this.tvMain = new System.Windows.Forms.TreeView();
      this.pnlSearch = new System.Windows.Forms.Panel();
      this.btnSearch = new System.Windows.Forms.Button();
      this.txtSearch = new System.Windows.Forms.TextBox();
      this.panelControl2 = new System.Windows.Forms.Panel();
      this.txtJson = new System.Windows.Forms.TextBox();
      this.pnlFooter.SuspendLayout();
      this.pnlLeft.SuspendLayout();
      this.pnlSearch.SuspendLayout();
      this.panelControl2.SuspendLayout();
      this.SuspendLayout();
      // 
      // pnlFooter
      // 
      this.pnlFooter.Controls.Add(this.btnNewJson);
      this.pnlFooter.Controls.Add(this.btnClose);
      this.pnlFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.pnlFooter.Location = new System.Drawing.Point(0, 597);
      this.pnlFooter.Name = "pnlFooter";
      this.pnlFooter.Size = new System.Drawing.Size(1016, 39);
      this.pnlFooter.TabIndex = 2;
      // 
      // btnNewJson
      // 
      this.btnNewJson.Location = new System.Drawing.Point(7, 6);
      this.btnNewJson.Name = "btnNewJson";
      this.btnNewJson.Size = new System.Drawing.Size(75, 23);
      this.btnNewJson.TabIndex = 1;
      this.btnNewJson.Text = "New Json";
      this.btnNewJson.Click += new System.EventHandler(this.btnNewJson_Click);
      // 
      // btnClose
      // 
      this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnClose.Location = new System.Drawing.Point(931, 8);
      this.btnClose.Name = "btnClose";
      this.btnClose.Size = new System.Drawing.Size(75, 23);
      this.btnClose.TabIndex = 0;
      this.btnClose.Text = "Close";
      this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
      // 
      // pnlLeft
      // 
      this.pnlLeft.Controls.Add(this.tvMain);
      this.pnlLeft.Controls.Add(this.pnlSearch);
      this.pnlLeft.Dock = System.Windows.Forms.DockStyle.Left;
      this.pnlLeft.Location = new System.Drawing.Point(0, 0);
      this.pnlLeft.Name = "pnlLeft";
      this.pnlLeft.Size = new System.Drawing.Size(371, 597);
      this.pnlLeft.TabIndex = 3;
      // 
      // tvMain
      // 
      this.tvMain.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tvMain.Location = new System.Drawing.Point(0, 35);
      this.tvMain.Name = "tvMain";
      this.tvMain.Size = new System.Drawing.Size(371, 562);
      this.tvMain.TabIndex = 0;
      this.tvMain.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvMain_AfterSelect);
      // 
      // pnlSearch
      // 
      this.pnlSearch.Controls.Add(this.btnSearch);
      this.pnlSearch.Controls.Add(this.txtSearch);
      this.pnlSearch.Dock = System.Windows.Forms.DockStyle.Top;
      this.pnlSearch.Location = new System.Drawing.Point(0, 0);
      this.pnlSearch.Name = "pnlSearch";
      this.pnlSearch.Size = new System.Drawing.Size(371, 35);
      this.pnlSearch.TabIndex = 1;
      // 
      // btnSearch
      // 
      this.btnSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnSearch.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnSearch.Location = new System.Drawing.Point(3, 5);
      this.btnSearch.Name = "btnSearch";
      this.btnSearch.Size = new System.Drawing.Size(75, 23);
      this.btnSearch.TabIndex = 1;
      this.btnSearch.Text = "Search";
      this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
      // 
      // txtSearch
      // 
      this.txtSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.txtSearch.Location = new System.Drawing.Point(84, 7);
      this.txtSearch.Name = "txtSearch";
      this.txtSearch.Size = new System.Drawing.Size(280, 20);
      this.txtSearch.TabIndex = 0;
      this.txtSearch.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtSearch_KeyPress);
      // 
      // panelControl2
      // 
      this.panelControl2.Controls.Add(this.txtJson);
      this.panelControl2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panelControl2.Location = new System.Drawing.Point(371, 0);
      this.panelControl2.Name = "panelControl2";
      this.panelControl2.Size = new System.Drawing.Size(645, 597);
      this.panelControl2.TabIndex = 4;
      // 
      // txtJson
      // 
      this.txtJson.Dock = System.Windows.Forms.DockStyle.Fill;
      this.txtJson.Location = new System.Drawing.Point(0, 0);
      this.txtJson.Multiline = true;
      this.txtJson.Name = "txtJson";
      this.txtJson.ScrollBars = System.Windows.Forms.ScrollBars.Both;
      this.txtJson.Size = new System.Drawing.Size(645, 597);
      this.txtJson.TabIndex = 0;
      // 
      // dlgJsonViewer
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(1016, 636);
      this.Controls.Add(this.panelControl2);
      this.Controls.Add(this.pnlLeft);
      this.Controls.Add(this.pnlFooter);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.Name = "dlgJsonViewer";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "Json Viewer";
      this.pnlFooter.ResumeLayout(false);
      this.pnlLeft.ResumeLayout(false);
      this.pnlSearch.ResumeLayout(false);
      this.pnlSearch.PerformLayout();
      this.panelControl2.ResumeLayout(false);
      this.panelControl2.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel pnlFooter;
    private System.Windows.Forms.Button btnClose;
    private System.Windows.Forms.Panel pnlLeft;
    private System.Windows.Forms.TreeView tvMain;
    private System.Windows.Forms.Panel panelControl2;
    private System.Windows.Forms.TextBox txtJson;
    private System.Windows.Forms.Panel pnlSearch;
    private System.Windows.Forms.TextBox txtSearch;
    private System.Windows.Forms.Button btnSearch;
    private System.Windows.Forms.Button btnNewJson;
  }
}