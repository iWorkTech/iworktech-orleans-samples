using System;
using System.Data.Common;
using System.IO;
using System.Threading.Tasks;
using iWorkTech.Orleans.Interfaces;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Orleans;
using Orleans.Concurrency;

namespace iWorkTech.Orleans.Grains
{
    public class ConnectionStrings
    {
        public string DataConnectionString { get; set; }
        public string ReduxConnectionString { get; set; }
        public string SmtpConnectionString { get; set; }
    }


    [StatelessWorker]
    public class EmailGrain : Grain, IEmailGrain
    {
        private readonly ILogger<EmailGrain> logger;
        private readonly SmtpConnectionString smtpSettings;

        public EmailGrain(IOptions<ConnectionStrings> connectionStrings, ILogger<EmailGrain> logger)
        {
            smtpSettings = new SmtpConnectionString
            {
                ConnectionString = connectionStrings.Value.SmtpConnectionString
            };
            this.logger = logger;
        }

        public async Task SendEmail(Email email)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(smtpSettings.FromDisplayName, smtpSettings.FromAddress));
            email.To.ForEach(address => message.To.Add(MailboxAddress.Parse(address)));
            message.Subject = email.Subject.Replace('\r', ' ').Replace('\n', ' ');
            var body = new TextPart("html") {Text = email.MessageBody};
            if (email.Attachments != null)
            {
                var multipart = new Multipart("mixed")
                {
                    body
                };
                email.Attachments.ForEach(attachment =>
                {
                    var part = new MimePart(attachment.MimeType)
                    {
                        ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                        ContentTransferEncoding = ContentEncoding.Base64,
                        FileName = attachment.Name,
                        Content = new MimeContent(new MemoryStream(attachment.Data))
                    };
                    multipart.Add(part);
                });
                message.Body = multipart;
            }
            else
            {
                message.Body = body;
            }

            await Task.Run(() =>
            {
                try
                {
                    using (var client = new SmtpClient())
                    {
                        // For demo-purposes, accept all SSL certificates (in case the server supports STARTTLS)
                        client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                        client.Connect(smtpSettings.Host, smtpSettings.Port, false);

                        // Note: since we don't have an OAuth2 token, disable
                        // the XOAUTH2 authentication mechanism.
                        client.AuthenticationMechanisms.Remove("XOAUTH2");

                        // Note: only needed if the SMTP server requires authentication
                        client.Authenticate(smtpSettings.UserName, smtpSettings.Password);

                        client.Send(message);
                        client.Disconnect(true);
                    }
                }
                catch (Exception e)
                {
                    logger.LogError(e, "EmailGrain: Error sending email");
                }
            });
        }
    }


    public class SmtpConnectionString : DbConnectionStringBuilder
    {
        public string Host
        {
            get => TryGetValue("Host", out var host) ? host.ToString() : null;
            set => this["Host"] = value;
        }

        public int Port
        {
            get => TryGetValue("Port", out var val) && int.TryParse(val.ToString(), out var port) ? port : 587;
            set => this["Port"] = value;
        }

        public bool EnableSsl
        {
            get => !TryGetValue("EnableSsl", out var val) || !string.Equals(val.ToString(), "false", StringComparison.OrdinalIgnoreCase); //default = true
            set => this["EnableSsl"] = value;
        }

        public string UserName
        {
            get => TryGetValue("UserName", out var userName) ? userName.ToString() : null;
            set => this["UserName"] = value;
        }

        public string Password
        {
            get => TryGetValue("Password", out var password) ? password.ToString() : null;
            set => this["Password"] = value;
        }

        public string FromAddress
        {
            get => TryGetValue("FromAddress", out var fromAddress) ? fromAddress.ToString() : "maarten@sikkema.com";
            set => this["FromAddress"] = value;
        }

        public string FromDisplayName
        {
            get => TryGetValue("FromDisplayName", out var fromDisplayName) ? fromDisplayName.ToString() : "RROD";
            set => this["FromDisplayName"] = value;
        }
    }
}