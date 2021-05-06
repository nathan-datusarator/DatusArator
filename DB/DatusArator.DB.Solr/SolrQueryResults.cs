using System.Collections.Generic;

using DatusArator.Core.Json;

namespace DatusArator.DB.Solr {
  public class SolrQueryResults {
    public string Query { get; set; }
    public int NumFound { get; set; }
    public int Start { get; set; }
    public long QTime { get; set; }
    public List<JsonWrapper> Records { get; }

    public SolrQueryResults(JsonWrapper raw) {
      Query = raw.Get("responseHeader.params.q");
      NumFound = raw.GetAsInt("response.numFound", 0).Value;
      Start = raw.GetAsInt("response.start", 0).Value;
      QTime = raw.GetAsLong("responseHeader.QTime", 0L).Value;

      Records = raw.GetArrayAsObjects("response.docs");
    }
  }
}
