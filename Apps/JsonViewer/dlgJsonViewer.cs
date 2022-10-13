using DatusArator.Core;
using DatusArator.Core.Json;
using DatusArator.Core.Util;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace DatusArator.Windows.Dialogs {
  public partial class dlgJsonViewer : Form {
    private JsonWrapper fWrapper;
    private JsonWrapper fBase;

    public dlgJsonViewer(string json = "") {
      InitializeComponent();

      if (string.IsNullOrEmpty(json))
        btnNewJson_Click(null, null);
    }

    private void btnNewJson_Click(object sender, EventArgs e) {
      string json = DialogUtils.EditTextInMemo("Enter Json", "");

      if (string.IsNullOrEmpty(json)) {
        var fileName = DialogUtils.OpenFile("", "Json files (*.json)|*.json|All files (*.*)|*.*");
        if (!string.IsNullOrEmpty(fileName))
          json = FileUtils.FileToString(fileName);
      }

      while (json.StartsWith("{{")) {
        json = json.Substring(1, json.Length - 2);
      }

      if (json.StartsWith("["))
        json = "{ data: " + json + " }";

      if (json.StartsWith("[")) {
        var parsed = JsonUtils.ParseJsonArray(json);
        fBase = parsed[0];
      } else {
        fBase = new JsonWrapper(json);
      }

      btnSearch_Click(null, null);
    }

    private void btnSearch_Click(object sender, EventArgs e) {
      if (txtSearch.Text == "")
        fWrapper = fBase;
      else {
        var search = txtSearch.Text.ToUpper();

        fWrapper = new JsonWrapper();
        foreach (var value in fBase.GetValues()) {
          if (value.Value.ToString().ToUpper().Contains(search) || value.Key.ToUpper().Contains(search))
            fWrapper[value.Key] = value.Value;
        }
      }

      ParseJson();

      tvMain_AfterSelect(null, null);
    }

    private void tvMain_AfterSelect(object sender, TreeViewEventArgs e) {
      var node = tvMain.SelectedNode;

      string json = null;
      string path = (node != null) ? node.Tag.ToString() : "";
      if (node == null)
        json = fWrapper.ToJsonString();
      else if (node.Nodes.Count == 0)
        txtJson.Text = path + Environment.NewLine + Environment.NewLine + fWrapper.Get(path);
      else {
        json = new JsonWrapper(fWrapper, path).ToJsonString();
      }

      if (json != null)
        txtJson.Text = path + Environment.NewLine + Environment.NewLine + JsonUtils.FormatJsonTabbed(json);
    }

    #region Build Tree
    private readonly Dictionary<string, TreeNode> fParentNodes = new Dictionary<string, TreeNode>();

    private void ParseJson() {
      tvMain.Nodes.Clear();
      fParentNodes.Clear();

      var values = fWrapper.GetValues();

      foreach (var value in values)
        Put(value.Key, value.Value);
    }

    private void Put(string path, TypedBasicObject value) {
      string[] parts = path.Split('.');

      string parentPath = null;

      TreeNode node = null;
      foreach (var part in parts) {
        var current = StringUtils.Concat(parentPath, part, ".");
        if (!fParentNodes.ContainsKey(current)) {
          if (parentPath == null)
            node = tvMain.Nodes.Add(part);
          else {
            var parent = fParentNodes[parentPath];
            node = parent.Nodes.Add(part);
          }

          fParentNodes[current] = node;
          node.Tag = current;
        } else {
          node = fParentNodes[current];
        }

        parentPath = current;
      }

      if (node != null) {
        var display = StringUtils.Shorten(value.ToString(), 20);

        var key = parts[parts.Length - 1];
        if (key.Equals("D", StringComparison.InvariantCultureIgnoreCase) || key.EndsWith("Date", StringComparison.InvariantCultureIgnoreCase) || key.EndsWith("TS", StringComparison.InvariantCultureIgnoreCase)) {
          var date = value.GetAsDate();
          if (date != null)
            display = date.Value.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");
        }

        node.Text = node.Text + " [" + display + "]";
      }
    }
    #endregion

    private void txtSearch_KeyPress(object sender, KeyPressEventArgs e) {
      if (e.KeyChar == 13)
        btnSearch_Click(null, null);
    }

    private void btnClose_Click(object sender, EventArgs e) {
      Close();
    }
  }
}
