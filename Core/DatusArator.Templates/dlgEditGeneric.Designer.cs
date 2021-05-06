namespace Templates {
  partial class dlgEditGeneric {
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
      this.pnlCommand = new DevExpress.XtraEditors.PanelControl();
      this.btnCancel = new DevExpress.XtraEditors.SimpleButton();
      this.btnOK = new DevExpress.XtraEditors.SimpleButton();
      this.pnlHeader = new DevExpress.XtraEditors.PanelControl();
      this.pnlBody = new DevExpress.XtraEditors.PanelControl();
      ((System.ComponentModel.ISupportInitialize)(this.pnlCommand)).BeginInit();
      this.pnlCommand.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pnlHeader)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.pnlBody)).BeginInit();
      this.SuspendLayout();
      // 
      // pnlCommand
      // 
      this.pnlCommand.Controls.Add(this.btnCancel);
      this.pnlCommand.Controls.Add(this.btnOK);
      this.pnlCommand.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.pnlCommand.Location = new System.Drawing.Point(0, 459);
      this.pnlCommand.Name = "pnlCommand";
      this.pnlCommand.Size = new System.Drawing.Size(702, 39);
      this.pnlCommand.TabIndex = 1;
      // 
      // btnCancel
      // 
      this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(615, 8);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(75, 23);
      this.btnCancel.TabIndex = 1;
      this.btnCancel.Text = "Cancel";
      // 
      // btnOK
      // 
      this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnOK.Location = new System.Drawing.Point(534, 8);
      this.btnOK.Name = "btnOK";
      this.btnOK.Size = new System.Drawing.Size(75, 23);
      this.btnOK.TabIndex = 0;
      this.btnOK.Text = "OK";
      this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
      // 
      // pnlHeader
      // 
      this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
      this.pnlHeader.Location = new System.Drawing.Point(0, 0);
      this.pnlHeader.Name = "pnlHeader";
      this.pnlHeader.Size = new System.Drawing.Size(702, 44);
      this.pnlHeader.TabIndex = 2;
      // 
      // pnlBody
      // 
      this.pnlBody.Dock = System.Windows.Forms.DockStyle.Fill;
      this.pnlBody.Location = new System.Drawing.Point(0, 44);
      this.pnlBody.Name = "pnlBody";
      this.pnlBody.Size = new System.Drawing.Size(702, 415);
      this.pnlBody.TabIndex = 3;
      // 
      // dlgEditGeneric
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(702, 498);
      this.Controls.Add(this.pnlBody);
      this.Controls.Add(this.pnlHeader);
      this.Controls.Add(this.pnlCommand);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.Name = "dlgEditGeneric";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Title";
      ((System.ComponentModel.ISupportInitialize)(this.pnlCommand)).EndInit();
      this.pnlCommand.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.pnlHeader)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.pnlBody)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private DevExpress.XtraEditors.PanelControl pnlCommand;
    private DevExpress.XtraEditors.SimpleButton btnCancel;
    private DevExpress.XtraEditors.SimpleButton btnOK;
    private DevExpress.XtraEditors.PanelControl pnlHeader;
    private DevExpress.XtraEditors.PanelControl pnlBody;
  }
}