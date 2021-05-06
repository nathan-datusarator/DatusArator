namespace DatusArator.DB.TcpServer {
  partial class frmMain {
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
      this.pnlTop = new System.Windows.Forms.Panel();
      this.txtResults = new System.Windows.Forms.TextBox();
      this.SuspendLayout();
      // 
      // pnlTop
      // 
      this.pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
      this.pnlTop.Location = new System.Drawing.Point(0, 0);
      this.pnlTop.Name = "pnlTop";
      this.pnlTop.Size = new System.Drawing.Size(763, 83);
      this.pnlTop.TabIndex = 0;
      // 
      // txtResults
      // 
      this.txtResults.Dock = System.Windows.Forms.DockStyle.Fill;
      this.txtResults.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.txtResults.Location = new System.Drawing.Point(0, 83);
      this.txtResults.Multiline = true;
      this.txtResults.Name = "txtResults";
      this.txtResults.ScrollBars = System.Windows.Forms.ScrollBars.Both;
      this.txtResults.Size = new System.Drawing.Size(763, 357);
      this.txtResults.TabIndex = 9;
      this.txtResults.WordWrap = false;
      // 
      // frmMain
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(763, 440);
      this.Controls.Add(this.txtResults);
      this.Controls.Add(this.pnlTop);
      this.Name = "frmMain";
      this.Text = "Datus Arator DB Server";
      this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmMain_FormClosed);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion
    private System.Windows.Forms.Panel pnlTop;
    private System.Windows.Forms.TextBox txtResults;
  }
}

