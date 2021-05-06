using System.Collections.Generic;
using static System.String;

namespace DatusArator.Core.DAO {
  public class SchemaBuilder {
    public Schema Schema { get; private set; }

    public SchemaBuilder(Schema schema) {
      this.Schema = schema;
    }

    public bool HasField() {
      return !IsNullOrEmpty(fCurrent?.Field);
    }

    private SchemaField fCurrent;

    public SchemaBuilder TableName(string name) { Schema.TableName = name; return this; }
    public SchemaBuilder DisplayName(string value) { Schema.DisplayName = value; return this; }
    public SchemaBuilder Shard(string[] shard) { Schema.Shard = shard; return this; }
    public SchemaBuilder KeyFields(string[] keyFields) { Schema.KeyFields = keyFields; return this; }

    public SchemaBuilder Shard(string field) { Schema.Shard = new string[] { field }; return this; }
    public SchemaBuilder KeyField(string field) { Schema.KeyFields = new string[] { field }; return this; }

    public SchemaBuilder PrimaryKey() { Schema.PrimaryKey = fCurrent.Field; return this; }
    public SchemaBuilder Commit() { Schema.Fields.Add(fCurrent); fCurrent = null; return this; }
    
    public SchemaBuilder Field(string dataField) { fCurrent = new SchemaField() { Field = dataField }; return this; }
    public SchemaBuilder Display(string value) { fCurrent.Display = IsNullOrEmpty(value) ? null : value; return this; }
    public SchemaBuilder Type(string value) { fCurrent.Type = IsNullOrEmpty(value) ? null : value.ToUpper(); return this; }
    public SchemaBuilder Order(int? value) { fCurrent.Order = value; return this; }
    public SchemaBuilder Sequence(string value) { fCurrent.Sequence = IsNullOrEmpty(value) ? null : value; return this; }
    public SchemaBuilder Notes(string value) { fCurrent.Notes = IsNullOrEmpty(value) ? null : value; return this; }
    public SchemaBuilder IsArray(bool value) { fCurrent.IsArray = value; return this; }
    public SchemaBuilder ExportHide(bool value) { fCurrent.ExportHide = value; return this; }

    public SchemaBuilder GridVisible(bool value) { fCurrent.Grid.Visible = value; return this; }
    public SchemaBuilder GridWidth(int? value) { fCurrent.Grid.Width = value; return this; }
    public SchemaBuilder GridDisplayFormat(string value) { fCurrent.Grid.DisplayFormat = IsNullOrEmpty(value) ? null : value; return this; }
    public SchemaBuilder GridGroupFormat(string value) { fCurrent.Grid.GroupFormat = IsNullOrEmpty(value) ? null : value; return this; }
    public SchemaBuilder GridSortOrder(string value) { fCurrent.Grid.SortOrder = value; return this; }
    public SchemaBuilder GridInterval(int? value) { fCurrent.Grid.Interval = value; return this; }
    public SchemaBuilder GridIntervalMax(int? value) { fCurrent.Grid.IntervalMax = value; return this; }
    public SchemaBuilder GridIntervalFormat(string value) { fCurrent.Grid.IntervalFormat = IsNullOrEmpty(value) ? null : value; return this; }

    public SchemaBuilder EditGroupDef(string caption, string colDefs = null, string rowDefs = null, bool captionHidden = false) {
      Schema.EditGroupDefs.Add(new EditGroupDef(caption, colDefs, rowDefs, captionHidden)); return this;
    }
    public SchemaBuilder EditGroupDef(EditGroupDef value) { Schema.EditGroupDefs.Add(value); return this; }
    public SchemaBuilder EditGroups(EditGroupDef[] values) { Schema.EditGroupDefs = new List<EditGroupDef>(values); return this; }
    public SchemaBuilder EditGroups(List<EditGroupDef> values) { Schema.EditGroupDefs = new List<EditGroupDef>(values); return this; }
    public SchemaBuilder EditFormDims(int width, int height) {
      Schema.EditFormWidth = width;
      Schema.EditFormHeight = height;
      return this;
    }

    public SchemaBuilder EditRequired(bool value) { fCurrent.Edit.Required = value; return this; }
    public SchemaBuilder EditReadOnly(bool value) { fCurrent.Edit.ReadOnly = value; return this; }
    public SchemaBuilder EditNotVisible(bool value) { fCurrent.Edit.NotVisible = value; return this; }
    public SchemaBuilder EditDisabled(bool value) { fCurrent.Edit.Disabled = value; return this; }

    public SchemaBuilder EditGroup(string value) { fCurrent.Edit.Group = value; return this; }
    public SchemaBuilder EditRowColSpan(string value) { fCurrent.Edit.RowColSpan = value; return this; }
    public SchemaBuilder EditWidth(int? value) { fCurrent.Edit.Width = value; return this; }
    public SchemaBuilder EditHeight(int? value) { fCurrent.Edit.Height = value; return this; }
    public SchemaBuilder EditHideCaption(bool value) { fCurrent.Edit.HideCaption = value; return this; }

    public SchemaBuilder EditType(string value) { fCurrent.Edit.Type = value; return this; }
    public SchemaBuilder EditValue(string value) { fCurrent.Edit.Value = value; return this; }

    public SchemaBuilder EditValidate(string value) { fCurrent.Edit.Validate = value; return this; }
    public SchemaBuilder EditFormat(string value) { fCurrent.Edit.Format = value; return this; }
    public SchemaBuilder EditWarn(int? value) { fCurrent.Edit.Warn = value; return this; }

    public SchemaBuilder EditMin(int? value) { fCurrent.Edit.Min = value; return this; }
    public SchemaBuilder EditMax(int? value) { fCurrent.Edit.Max = value; return this; }

    public SchemaBuilder EditMapType(string value) { fCurrent.Edit.MapType = value; return this; }
    public SchemaBuilder EditMapSrc(string value) { fCurrent.Edit.MapSrc = value; return this; }
    public SchemaBuilder EditMapFilter(string value) { fCurrent.Edit.MapFilter = value; return this; }
    public SchemaBuilder EditMapValue(string key, string caption, bool notVisible = false) {
      fCurrent.Edit.AddMapValue(key, caption, notVisible);
      return this;
    }
  }
}
