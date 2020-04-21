using System.Collections.Generic;
using System.Net.Mail;
using AutoMapper;
using Loadshop.DomainServices.Loadshop.DataProvider.Entities;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Loadshop.DomainServices.Loadshop.Services
{
    public class EmailService : IEmailService
    {
        private readonly IMapper _mapper;
        private readonly IConfigurationRoot _configuration;
        private readonly bool _isDev = true;

        public EmailService(IMapper mapper, IConfigurationRoot configuration)
        {
            _mapper = mapper;
            _configuration = configuration;
            _isDev = _configuration["Region"].ToLower().Contains("dev");
        }

        public void SendMailMessage(NotificationMessageEntity notificationMessage, bool addBcc)
        {
            var message = new MailMessage();
            message.From = new MailAddress("KBXLOpportunities@kbxlogistics.com");
            message.Subject = notificationMessage.Subject;
            message.Body = notificationMessage.Message;
            message.IsBodyHtml = true;

            var emailAddressDelimiters = new char[2] { ';', ',' };
            if (!string.IsNullOrWhiteSpace(notificationMessage.To))
            {
                foreach (var item in notificationMessage.To.Split(emailAddressDelimiters))
                {
                    if (!string.IsNullOrWhiteSpace(item))
                    {
                        message.To.Add(new MailAddress(item));
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(notificationMessage.CC))
            {
                foreach (var item in notificationMessage.CC.Split(emailAddressDelimiters))
                {
                    if (!string.IsNullOrWhiteSpace(item))
                    {
                        message.CC.Add(new MailAddress(item));
                    }
                }
            }

            if (addBcc)
            {
                message.Bcc.Add(new MailAddress("KBXLOpportunities@kbxlogistics.com"));
            }

            if (_isDev)
            {
                RemoveNonKBXEmailsForDev(message);
                message.Subject = $"{message.Subject} ({_configuration["Region"]})";

                if (message.To.Count == 0)
                {
                    var devEmail = _configuration["DevEmail"];
                    if (!string.IsNullOrWhiteSpace(devEmail))
                    {
                        message.To.Add(new MailAddress(devEmail));
                    }
                }
            }

            if (message.To.Count > 0)
            {
                var client = new SmtpClient("imail.kochind.com");
                client.Send(message);
            }
        }

        public void RemoveNonKBXEmailsForDev(MailMessage message)
        {
            if (message != null)
            {
                var to = RemoveNonKBXEmailsForDev(message.To);
                message.To.Clear();
                if (to != null)
                {
                    foreach (var item in to)
                    {
                        if (!string.IsNullOrWhiteSpace(item))
                        {
                            message.To.Add(new MailAddress(item));
                        }
                    }
                }

                var cc = RemoveNonKBXEmailsForDev(message.CC);
                message.CC.Clear();
                if (cc != null)
                {
                    foreach (var item in cc)
                    {
                        if (!string.IsNullOrWhiteSpace(item))
                        {
                            message.CC.Add(new MailAddress(item));
                        }
                    }
                }
            }
        }

        public List<string> RemoveNonKBXEmailsForDev(MailAddressCollection mailAddresses)
        {
            var result = new List<string>();
            if (mailAddresses != null)
            {
                var kbxEmailAddress = "@kbxlogistics.com";
                foreach (var item in mailAddresses)
                {
                    if (item != null && !string.IsNullOrWhiteSpace(item.Address))
                    {
                        if (item.Address.ToLower().Contains(kbxEmailAddress))
                        {
                            result.Add(item.Address);
                        }
                    }
                }
            }

            return result;
        }
    }
}
