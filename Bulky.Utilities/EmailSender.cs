using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Bulky.Utilities
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            //Logic to send email
            return Task.CompletedTask;
        }
    }
}
