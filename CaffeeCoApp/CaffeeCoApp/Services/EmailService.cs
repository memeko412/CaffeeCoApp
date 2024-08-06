using Newtonsoft.Json.Linq;
using sib_api_v3_sdk.Api;
using sib_api_v3_sdk.Model;
using System.Diagnostics;

namespace CaffeeCoApp.Services
{
    public class EmailService
    {
        public static void SendEmail(string senderName, string senderEmail, 
            string toEmail, string toName, string textContent, string subject)
        {
            var apiInstance = new TransactionalEmailsApi();
            SendSmtpEmailSender Email = new SendSmtpEmailSender(senderName, senderEmail);
            SendSmtpEmailTo smtpEmailTo = new SendSmtpEmailTo(toEmail, toName);
            List<SendSmtpEmailTo> To = new List<SendSmtpEmailTo>();
            To.Add(smtpEmailTo);

            try
            {
                var sendSmtpEmail = new SendSmtpEmail(Email, To, null, null, null, textContent, subject);
                CreateSmtpEmail result = apiInstance.SendTransacEmail(sendSmtpEmail);
                Console.WriteLine("Email Sender OK: \n" + result.ToJson());
            }
            catch (Exception e)
            {
                Console.WriteLine("Email Service Failure:\n" + e.Message);
            }
        }

        public static void BatchSendEmailsWithAttachment(string senderName, string senderEmail, List<string> toEmailsList, List<string> toNameList, string textContent, string subject, IFormFile attachmentFile)
        {
            var apiInstance = new TransactionalEmailsApi();
            SendSmtpEmailSender Email = new SendSmtpEmailSender(senderName, senderEmail);

            List<SendSmtpEmailTo> To = new List<SendSmtpEmailTo>();
            for (int i = 0; i < toEmailsList.Count; i++ )
            {
                string ToEmail = toEmailsList[i];
                string ToName = toNameList[i];
                SendSmtpEmailTo smtpEmailTo = new SendSmtpEmailTo(ToEmail, ToName);
                To.Add(smtpEmailTo);
            }

            string TextContent = textContent;
            string Subject = subject;


            string base64Content = null;
            string AttachmentName = null;
            if (attachmentFile != null && attachmentFile.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    attachmentFile.CopyTo(ms);
                    byte[] fileBytes = ms.ToArray();
                    base64Content = Convert.ToBase64String(fileBytes);
                    AttachmentName = attachmentFile.FileName;
                }
            }

            // Create the attachment object
            SendSmtpEmailAttachment attachmentContent = null;
            if (!string.IsNullOrEmpty(base64Content))
            {
                attachmentContent = new SendSmtpEmailAttachment(null, Convert.FromBase64String(base64Content), AttachmentName);
            }

            List<SendSmtpEmailAttachment> Attachments = new List<SendSmtpEmailAttachment>();
            if (attachmentContent != null)
            {
                Attachments.Add(attachmentContent);
            }

            try
            {
                if(Attachments.Count == 0)
                {
                    Console.WriteLine("No attachment found");
                    var sendSmtpEmail = new SendSmtpEmail(Email, To, null, null, null, TextContent, Subject);
                    CreateSmtpEmail result = apiInstance.SendTransacEmail(sendSmtpEmail);
                    Debug.WriteLine(result.ToJson());
                    Console.WriteLine(result.ToJson());
                } else
                {
                    var sendSmtpEmail = new SendSmtpEmail(Email, To, null, null, null, TextContent, Subject, null, Attachments);
                    CreateSmtpEmail result = apiInstance.SendTransacEmail(sendSmtpEmail);
                    Debug.WriteLine(result.ToJson());
                    Console.WriteLine(result.ToJson());
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                Console.WriteLine(e.Message);
            }
        }
    }
}
