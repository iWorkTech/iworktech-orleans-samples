using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iWorkTech.Orleans.Web.Core.Identity.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
