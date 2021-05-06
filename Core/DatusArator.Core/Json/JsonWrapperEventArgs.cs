using System;

namespace DatusArator.Core.Json {
  public delegate void JsonWrapperEventHandler(object sender, JsonWrapperEventArgs e);
  public class JsonWrapperEventArgs : EventArgs {
    public JsonWrapper Value { get; set; }
    
    public JsonWrapperEventArgs(JsonWrapper value) {
      this.Value = value;
    }

    public JsonWrapperEventArgs(string json) {
      this.Value = new JsonWrapper(json);
    }

    public JsonWrapperEventArgs(string key, string value) {
      this.Value = new JsonWrapper().Put(key, value);
    }

    public JsonWrapperEventArgs(Object o) {
      var json = JsonUtils.ObjectToJson(o);
      this.Value = new JsonWrapper(json);
    }
  }
}
