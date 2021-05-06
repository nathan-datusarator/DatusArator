using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;

namespace DatusArator.Core.Util {
  public static class MailUtils {
    public static string mailServer { get; set; }
    public static int mailPort { get; set; }
    public static string mailUserId { get; set; }
    public static string mailPassword { get; set; }
    public static bool mailEnableSSL { get; set; }

    public static void initialize(String server, String port, String userId = null, String password = null, bool enableSSL = false) {
      mailServer = server;
      SetPort(port);
      mailUserId = userId;
      mailPassword = password;
      mailEnableSSL = enableSSL;
    }

    public static void SetPort(string port) {
      mailPort = (port != null) ? Convert.ToInt32(port) : -1;
    }

    public static void SendMail(String to, String from, String cc, String subject, String body, List<String> attachments = null, bool htmlBody = false, SendCompletedEventHandler mailSent = null) {
      SendMail(to, from, from, cc, subject, body, attachments, htmlBody, mailSent);
    }

    public static void SendMail(String to, String from, String replyTo, String cc, String subject, String body, List<String> attachments = null, bool htmlBody = false, SendCompletedEventHandler mailSent = null) {
      MailMessage mailObject = new MailMessage();
      if (attachments != null)
        foreach (var attachment in attachments)
          mailObject.Attachments.Add(new System.Net.Mail.Attachment(attachment));
      SendMail(mailObject, to, from, replyTo, cc, subject, body, htmlBody, mailSent);
    }

    public static void SendMail(String to, String from, String replyTo, String cc, String subject, String body, List<byte[]> attachments, List<String> attachmentNames, bool htmlBody = false, SendCompletedEventHandler mailSent = null) {
      MailMessage mailObject = new MailMessage();
      if (attachments != null)
        foreach (var attachment in attachments)
          mailObject.Attachments.Add(new System.Net.Mail.Attachment(new MemoryStream(attachment), attachmentNames[attachments.IndexOf(attachment)]));
      SendMail(mailObject, to, from, replyTo, cc, subject, body, htmlBody, mailSent);
    }

    public static void SendMail(MailMessage mailObject, String to, String from, String replyTo, String cc, String subject, String body, bool htmlBody = false, SendCompletedEventHandler mailSent = null) {
      try {
        mailObject.From = new MailAddress(from);
        mailObject.ReplyToList.Add(replyTo);

        mailObject.To.Add(to);
        mailObject.Subject = subject;
        mailObject.Body = body;

        mailObject.IsBodyHtml = htmlBody;

        if (cc != null)
          mailObject.CC.Add(cc);

        using (var smtpServer = new SmtpClient(mailServer, Math.Max(mailPort, 0))) {
          smtpServer.DeliveryMethod = SmtpDeliveryMethod.Network;

          smtpServer.SendCompleted += new SendCompletedEventHandler(SendCompletedCallback);
          if (mailSent != null) {
            smtpServer.SendCompleted += mailSent;
          }

          if (mailUserId != null)
            smtpServer.Credentials = new System.Net.NetworkCredential(mailUserId, mailPassword);
          smtpServer.UseDefaultCredentials = false;
          smtpServer.EnableSsl = mailEnableSSL;

          if (mailObject.Attachments.Count > 0) {
            TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
            int secondsSinceEpoch = (int)t.TotalSeconds;
            BaseLogger.LogInfo(String.Format("[{0}] SendAsync.", secondsSinceEpoch));
            smtpServer.SendAsync(mailObject, secondsSinceEpoch.ToString());
          } else
            smtpServer.Send(mailObject);
        }

        //BaseLogger.LogInfo(String.Format("[Mail] {0}:{1} - {2} ({3}) -> SSL: {4}", smtpServer.Host, smtpServer.Port, mailUserId, mailPassword, mailEnableSSL));
      } catch (Exception ex) {
        BaseLogger.LogError("Error sending message: " + ex.Message);
      }
    }
    private static void SendCompletedCallback(object sender, AsyncCompletedEventArgs e) {
      String token = (string)e.UserState;

      if (e.Cancelled)
        BaseLogger.LogError(String.Format("[{0}] Send canceled.", token));
      if (e.Error != null)
        BaseLogger.LogError(String.Format("[{0}] {1}", token, e.Error.ToString()));
      else
        BaseLogger.LogInfo(String.Format("[{0}] Message sent.", token));
    }
  }
}
