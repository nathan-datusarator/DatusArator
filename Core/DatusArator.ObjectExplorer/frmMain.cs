using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

using DatusArator.Core.ObjectStore;
using DatusArator.Core.Util;
using DatusArator.DB.Lucene.LuceneSearch;
using DatusArator.Windows.Dialogs;

namespace DatusArator.ObjectExplorer {
  public partial class frmMain : Form {
    private IDatusAratorObjectStore fStore;

    public frmMain() {
      InitializeComponent();
    }

    private void btnOpen_Click(object sender, EventArgs e) {
      if (!Directory.Exists(txtDirectory.Text))
        return;

      if (fStore != null) {
        fStore.Dispose();
        fStore = null;
      }

      var dir = FileUtils.AddSlash(txtDirectory.Text);
      if (File.Exists(dir + "segments.gen")) {
        Init(new DatusAratorLuceneObjectStore(dir));
      } else
        Init(new DatusAratorFileObjectStore(dir));
    }

    private void txtDirectory_KeyPress(object sender, KeyPressEventArgs e) {
      if (e.KeyChar == 13)
        btnOpen_Click(null, null);
    }

    private void btnChooseDirectory_Click(object sender, EventArgs e) {
      dlgFolder.SelectedPath = txtDirectory.Text;
      if (dlgFolder.ShowDialog() == DialogResult.OK)
        txtDirectory.Text = dlgFolder.SelectedPath;
    }

    private void Init(IDatusAratorObjectStore store) {
      fStore = store;

      var buckets = fStore.Buckets("");

      var data = new Dictionary<string, TreeViewEntry>();
      foreach (var bucket in buckets) {
        var entry = new TreeViewEntry(bucket);
        data[bucket] = entry;

        AddParents(data, entry);
      }

      var list = new List<TreeViewEntry>(data.Values);
      tvMain.BeginUpdate();
      tvMain.DataSource = list;
      tvMain.CollapseAll();
      tvMain.EndUpdate();
    }

    private void AddParents(Dictionary<string, TreeViewEntry> data, TreeViewEntry entry) {
      if (string.IsNullOrEmpty(entry.ParentID) || data.ContainsKey(entry.ParentID))
        return;

      var parent = new TreeViewEntry(entry.ParentID);
      data[parent.ID] = parent;
      AddParents(data, parent);
    }

    private string CurrentBucket() {
      return tvMain.FocusedNode[tvMain.KeyFieldName]?.ToString();
    }

    private string fLastFocusedRow = null;
    private void tvMain_FocusedNodeChanged(object sender, DevExpress.XtraTreeList.FocusedNodeChangedEventArgs e) {
      var bucket = CurrentBucket();
      if (bucket == fLastFocusedRow)
        return;

      fLastFocusedRow = bucket;

      lbObjects.DataSource = null;
      if (string.IsNullOrEmpty(fLastFocusedRow))
        return;

      var ids = fStore.Ids(fLastFocusedRow);
      if ((ids?.Count ?? 0) == 0)
        return;

      ids.Sort();

      lbObjects.DataSource = ids;
    }

    private void lbObjects_SelectedIndexChanged(object sender, EventArgs e) {
      var file = lbObjects.SelectedValue?.ToString();

      if (string.IsNullOrEmpty(lbObjects.SelectedValue?.ToString())) {
        txtObject.Visible = true;
        txtObject.Text = "";
        pbObject.Image = null;
      } else {
        if (file.EndsWith("jpg")) {
          txtObject.Visible = false;
          pbObject.Image = Image.FromStream(GeneralUtils.BytesToStream(fStore.GetBin(CurrentBucket(), file)));
        } else {
          pbObject.Image = null;
          txtObject.Visible = true;
          txtObject.Text = fStore.Get(CurrentBucket(), file);
        }
      }
    }

    private void btnJsonViewer_Click(object sender, EventArgs e) {
      var json = txtObject.Text;
      if (!json.StartsWith("{"))
        json = "";

      new dlgJsonViewer(json).Show();
    }
  }

  class TreeViewEntry {
    public string ID { get; set; }
    public string ParentID { get; set; }
    public string Text { get; set; }

    public TreeViewEntry(string value) {
      int index = value.LastIndexOf(":");
      if (index > 0) {
        ID = value;
        Text = value.Substring(index + 1);
        ParentID = value.Substring(0, index);
      } else {
        ID = value;
        Text = value;
        ParentID = null;
      }
    }

    public override string ToString() {
      return Text;
    }
  }
}
