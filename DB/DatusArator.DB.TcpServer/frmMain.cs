using System;
using System.Net;
using System.Windows.Forms;

using DatusArator.Core.Http.Server;
using DatusArator.Core.Json;
using DatusArator.Core.ObjectStore;
using DatusArator.Core.Util;
using DatusArator.DB.Lucene.LuceneSearch;
using DatusArator.DB.TcpServer.Handlers;

namespace DatusArator.DB.TcpServer {
  public partial class frmMain : Form {
    public static string nl = Environment.NewLine;

    public frmMain() {
      InitializeComponent();

      GeneralUtils.StartPulse();

      DatusAratorObjectStoreFactory.Register(new DatusAratorLuceneObjectStore(null));

      StartServer();
    }

    private void frmMain_FormClosed(object sender, FormClosedEventArgs e) {
      GeneralUtils.StopPulse();
    }

    public static SimpleHTTPServer fHttpServer;

    private bool StartServer() {
      if (fHttpServer != null) return false;

      //      var ipAddress = WebUtils.GetIpAddress(string.Empty);
      //      var hostName = System.Net.Dns.GetHostName();

      //fHttpServer = new SimpleHTTPServer("+", 4201, false);
      fHttpServer = new SimpleHTTPServer("localhost", 4202, false);

      fHttpServer.AddHandler("", new SimpleCommandHandler(OnBaseHandler));
      fHttpServer.AddHandler(@"api/session", new SimpleJsonCommandHandler(OnSessionCommand));
      fHttpServer.AddHandler(@"api/data", new SimpleJsonCommandHandler(OnDataCommand));

      return true;
    }

    private object OnBaseHandler(string command, HttpListenerRequest request) {
      var message = SimpleHTTPServer.RequestToJsonWrapper(request);

      BeginInvoke(new Action(delegate () {
        txtResults.AppendText("BASE:   " + command + " : " + message.ToJsonString() + nl);
      }));

      return new JsonWrapper().Put("error", "Unknown Command: " + command);
    }

    private JsonWrapper OnSessionCommand(HttpListenerRequest request) {
      var message = SimpleHTTPServer.RequestToJsonWrapper(request);

      BeginInvoke(new Action(delegate () {
        txtResults.AppendText("SESSION: " + message.ToJsonString() + nl);
      }));

      return RequestHandler.ProcessSession(message);
    }

    private JsonWrapper OnDataCommand(HttpListenerRequest request) {
      var message = SimpleHTTPServer.RequestToJsonWrapper(request);

      BeginInvoke(new Action(delegate () {
        txtResults.AppendText("DATA:    " + message.ToJsonString() + nl);
      }));

      return RequestHandler.ProcessData(message);
    }
  }
}
