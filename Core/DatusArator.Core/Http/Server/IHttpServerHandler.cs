using System.Net;

namespace DatusArator.Core.Http.Server {
  public interface IHttpServerHandler {
    bool Process(string command, HttpListenerContext context);
  }
}
