namespace DatusArator.ObjectExplorer {
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
      this.dlgFolder = new System.Windows.Forms.FolderBrowserDialog();
      this.pnlCommand = new System.Windows.Forms.Panel();
      this.btnJsonViewer = new System.Windows.Forms.Button();
      this.btnOpen = new System.Windows.Forms.Button();
      this.txtDirectory = new System.Windows.Forms.TextBox();
      this.btnChooseDirectory = new System.Windows.Forms.Button();
      this.pnlBody = new System.Windows.Forms.Panel();
      this.txtObject = new System.Windows.Forms.TextBox();
      this.pbObject = new System.Windows.Forms.PictureBox();
      this.lbObjects = new System.Windows.Forms.ListBox();
      this.tvMain = new DevExpress.XtraTreeList.TreeList();
      this.pnlCommand.SuspendLayout();
      this.pnlBody.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pbObject)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.tvMain)).BeginInit();
      this.SuspendLayout();
      // 
      // pnlCommand
      // 
      this.pnlCommand.Controls.Add(this.btnJsonViewer);
      this.pnlCommand.Controls.Add(this.btnOpen);
      this.pnlCommand.Controls.Add(this.txtDirectory);
      this.pnlCommand.Controls.Add(this.btnChooseDirectory);
      this.pnlCommand.Dock = System.Windows.Forms.DockStyle.Top;
      this.pnlCommand.Location = new System.Drawing.Point(0, 0);
      this.pnlCommand.Name = "pnlCommand";
      this.pnlCommand.Size = new System.Drawing.Size(964, 40);
      this.pnlCommand.TabIndex = 1;
      // 
      // btnJsonViewer
      // 
      this.btnJsonViewer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnJsonViewer.Location = new System.Drawing.Point(884, 8);
      this.btnJsonViewer.Name = "btnJsonViewer";
      this.btnJsonViewer.Size = new System.Drawing.Size(75, 23);
      this.btnJsonViewer.TabIndex = 4;
      this.btnJsonViewer.Text = "Json";
      this.btnJsonViewer.UseVisualStyleBackColor = true;
      this.btnJsonViewer.Click += new System.EventHandler(this.btnJsonViewer_Click);
      // 
      // btnOpen
      // 
      this.btnOpen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnOpen.Location = new System.Drawing.Point(803, 8);
      this.btnOpen.Name = "btnOpen";
      this.btnOpen.Size = new System.Drawing.Size(75, 23);
      this.btnOpen.TabIndex = 3;
      this.btnOpen.Text = "Open";
      this.btnOpen.UseVisualStyleBackColor = true;
      this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
      // 
      // txtDirectory
      // 
      this.txtDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.txtDirectory.Location = new System.Drawing.Point(93, 10);
      this.txtDirectory.Name = "txtDirectory";
      this.txtDirectory.Size = new System.Drawing.Size(704, 20);
      this.txtDirectory.TabIndex = 2;
      this.txtDirectory.Text = "c:\\temp\\ARAnalytics\\AirBnB";
      this.txtDirectory.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtDirectory_KeyPress);
      // 
      // btnChooseDirectory
      // 
      this.btnChooseDirectory.Location = new System.Drawing.Point(12, 8);
      this.btnChooseDirectory.Name = "btnChooseDirectory";
      this.btnChooseDirectory.Size = new System.Drawing.Size(75, 23);
      this.btnChooseDirectory.TabIndex = 1;
      this.btnChooseDirectory.Text = "Choose:";
      this.btnChooseDirectory.UseVisualStyleBackColor = true;
      this.btnChooseDirectory.Click += new System.EventHandler(this.btnChooseDirectory_Click);
      // 
      // pnlBody
      // 
      this.pnlBody.Controls.Add(this.txtObject);
      this.pnlBody.Controls.Add(this.pbObject);
      this.pnlBody.Controls.Add(this.lbObjects);
      this.pnlBody.Controls.Add(this.tvMain);
      this.pnlBody.Dock = System.Windows.Forms.DockStyle.Fill;
      this.pnlBody.Location = new System.Drawing.Point(0, 40);
      this.pnlBody.Name = "pnlBody";
      this.pnlBody.Size = new System.Drawing.Size(964, 547);
      this.pnlBody.TabIndex = 2;
      // 
      // txtObject
      // 
      this.txtObject.Dock = System.Windows.Forms.DockStyle.Fill;
      this.txtObject.Location = new System.Drawing.Point(395, 0);
      this.txtObject.Multiline = true;
      this.txtObject.Name = "txtObject";
      this.txtObject.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
      this.txtObject.Size = new System.Drawing.Size(569, 547);
      this.txtObject.TabIndex = 4;
      // 
      // pbObject
      // 
      this.pbObject.Dock = System.Windows.Forms.DockStyle.Fill;
      this.pbObject.Location = new System.Drawing.Point(395, 0);
      this.pbObject.Name = "pbObject";
      this.pbObject.Size = new System.Drawing.Size(569, 547);
      this.pbObject.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
      this.pbObject.TabIndex = 5;
      this.pbObject.TabStop = false;
      // 
      // lbObjects
      // 
      this.lbObjects.Dock = System.Windows.Forms.DockStyle.Left;
      this.lbObjects.FormattingEnabled = true;
      this.lbObjects.Location = new System.Drawing.Point(241, 0);
      this.lbObjects.Name = "lbObjects";
      this.lbObjects.Size = new System.Drawing.Size(154, 547);
      this.lbObjects.TabIndex = 1;
      this.lbObjects.SelectedIndexChanged += new System.EventHandler(this.lbObjects_SelectedIndexChanged);
      // 
      // tvMain
      // 
      this.tvMain.Dock = System.Windows.Forms.DockStyle.Left;
      this.tvMain.Location = new System.Drawing.Point(0, 0);
      this.tvMain.Name = "tvMain";
      this.tvMain.OptionsBehavior.Editable = false;
      this.tvMain.OptionsFilter.FilterMode = DevExpress.XtraTreeList.FilterMode.Extended;
      this.tvMain.Size = new System.Drawing.Size(241, 547);
      this.tvMain.TabIndex = 0;
      this.tvMain.FocusedNodeChanged += new DevExpress.XtraTreeList.FocusedNodeChangedEventHandler(this.tvMain_FocusedNodeChanged);
      // 
      // frmMain
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(964, 587);
      this.Controls.Add(this.pnlBody);
      this.Controls.Add(this.pnlCommand);
      this.Name = "frmMain";
      this.Text = "Browse Datus Arator Object Store";
      this.pnlCommand.ResumeLayout(false);
      this.pnlCommand.PerformLayout();
      this.pnlBody.ResumeLayout(false);
      this.pnlBody.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pbObject)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.tvMain)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.FolderBrowserDialog dlgFolder;
    private System.Windows.Forms.Panel pnlCommand;
    private System.Windows.Forms.Button btnChooseDirectory;
    private System.Windows.Forms.Panel pnlBody;
    private System.Windows.Forms.Button btnOpen;
    private System.Windows.Forms.TextBox txtDirectory;
    private DevExpress.XtraTreeList.TreeList tvMain;
    private System.Windows.Forms.ListBox lbObjects;
    private System.Windows.Forms.TextBox txtObject;
    private System.Windows.Forms.PictureBox pbObject;
    private System.Windows.Forms.Button btnJsonViewer;
  }
}

