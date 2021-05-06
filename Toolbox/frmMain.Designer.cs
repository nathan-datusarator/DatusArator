namespace Toolbox {
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
      this.txtResults = new System.Windows.Forms.TextBox();
      this.pnlHeader = new System.Windows.Forms.Panel();
      this.btnSpacingCSS = new System.Windows.Forms.Button();
      this.btnTourCompiler = new System.Windows.Forms.Button();
      this.btnUk = new System.Windows.Forms.Button();
      this.btnInterests = new System.Windows.Forms.Button();
      this.btnUS = new System.Windows.Forms.Button();
      this.btnWorldMap = new System.Windows.Forms.Button();
      this.pnlHeader.SuspendLayout();
      this.SuspendLayout();
      // 
      // txtResults
      // 
      this.txtResults.Dock = System.Windows.Forms.DockStyle.Fill;
      this.txtResults.Location = new System.Drawing.Point(0, 77);
      this.txtResults.Multiline = true;
      this.txtResults.Name = "txtResults";
      this.txtResults.ScrollBars = System.Windows.Forms.ScrollBars.Both;
      this.txtResults.Size = new System.Drawing.Size(853, 411);
      this.txtResults.TabIndex = 1;
      // 
      // pnlHeader
      // 
      this.pnlHeader.Controls.Add(this.btnSpacingCSS);
      this.pnlHeader.Controls.Add(this.btnTourCompiler);
      this.pnlHeader.Controls.Add(this.btnUk);
      this.pnlHeader.Controls.Add(this.btnInterests);
      this.pnlHeader.Controls.Add(this.btnUS);
      this.pnlHeader.Controls.Add(this.btnWorldMap);
      this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
      this.pnlHeader.Location = new System.Drawing.Point(0, 0);
      this.pnlHeader.Name = "pnlHeader";
      this.pnlHeader.Size = new System.Drawing.Size(853, 77);
      this.pnlHeader.TabIndex = 2;
      // 
      // btnSpacingCSS
      // 
      this.btnSpacingCSS.Location = new System.Drawing.Point(340, 12);
      this.btnSpacingCSS.Name = "btnSpacingCSS";
      this.btnSpacingCSS.Size = new System.Drawing.Size(75, 23);
      this.btnSpacingCSS.TabIndex = 6;
      this.btnSpacingCSS.Text = "CSS";
      this.btnSpacingCSS.UseVisualStyleBackColor = true;
      this.btnSpacingCSS.Click += new System.EventHandler(this.btnSpacingCSS_Click);
      // 
      // btnTourCompiler
      // 
      this.btnTourCompiler.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnTourCompiler.Location = new System.Drawing.Point(189, 12);
      this.btnTourCompiler.Name = "btnTourCompiler";
      this.btnTourCompiler.Size = new System.Drawing.Size(75, 23);
      this.btnTourCompiler.TabIndex = 5;
      this.btnTourCompiler.Text = "Tours";
      this.btnTourCompiler.UseVisualStyleBackColor = true;
      this.btnTourCompiler.Click += new System.EventHandler(this.btnTourCompiler_Click);
      // 
      // btnUk
      // 
      this.btnUk.Location = new System.Drawing.Point(108, 12);
      this.btnUk.Name = "btnUk";
      this.btnUk.Size = new System.Drawing.Size(75, 23);
      this.btnUk.TabIndex = 4;
      this.btnUk.Text = "Uk Map";
      this.btnUk.UseVisualStyleBackColor = true;
      this.btnUk.Click += new System.EventHandler(this.btnUK_Click);
      // 
      // btnInterests
      // 
      this.btnInterests.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnInterests.Location = new System.Drawing.Point(189, 41);
      this.btnInterests.Name = "btnInterests";
      this.btnInterests.Size = new System.Drawing.Size(75, 23);
      this.btnInterests.TabIndex = 3;
      this.btnInterests.Text = "Interests";
      this.btnInterests.UseVisualStyleBackColor = true;
      this.btnInterests.Click += new System.EventHandler(this.btnInterests_Click);
      // 
      // btnUS
      // 
      this.btnUS.Location = new System.Drawing.Point(27, 41);
      this.btnUS.Name = "btnUS";
      this.btnUS.Size = new System.Drawing.Size(75, 23);
      this.btnUS.TabIndex = 2;
      this.btnUS.Text = "US Map";
      this.btnUS.UseVisualStyleBackColor = true;
      this.btnUS.Click += new System.EventHandler(this.btnUSMap_Click);
      // 
      // btnWorldMap
      // 
      this.btnWorldMap.Location = new System.Drawing.Point(27, 12);
      this.btnWorldMap.Name = "btnWorldMap";
      this.btnWorldMap.Size = new System.Drawing.Size(75, 23);
      this.btnWorldMap.TabIndex = 1;
      this.btnWorldMap.Text = "World Map";
      this.btnWorldMap.UseVisualStyleBackColor = true;
      this.btnWorldMap.Click += new System.EventHandler(this.btnWorldMap_Click);
      // 
      // frmMain
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(853, 488);
      this.Controls.Add(this.txtResults);
      this.Controls.Add(this.pnlHeader);
      this.Name = "frmMain";
      this.Text = "Datus Arator Toolbox";
      this.pnlHeader.ResumeLayout(false);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TextBox txtResults;
    private System.Windows.Forms.Panel pnlHeader;
    private System.Windows.Forms.Button btnWorldMap;
    private System.Windows.Forms.Button btnUS;
    private System.Windows.Forms.Button btnInterests;
    private System.Windows.Forms.Button btnUk;
    private System.Windows.Forms.Button btnTourCompiler;
    private System.Windows.Forms.Button btnSpacingCSS;
  }
}

