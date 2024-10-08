﻿using System.Net.Mail;
using Clean.Architecture.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Clean.Architecture.Infrastructure.Email;

/// <summary>
/// MimeKit is recommended over this now:
/// https://weblogs.asp.net/sreejukg/system-net-mail-smtpclient-is-not-recommended-anymore-what-is-the-alternative
/// </summary>
public class SmtpEmailSender(
  ILogger<SmtpEmailSender> logger,
  IOptions<MailserverConfiguration> mailserverOptions)
  : IEmailSender
{
  private readonly MailserverConfiguration _mailserverConfiguration = mailserverOptions.Value!;

  public async Task SendEmailAsync(string to, string from, string subject, string body)
  {
   using var emailClient = new SmtpClient(_mailserverConfiguration.Hostname, _mailserverConfiguration.Port);

   using  var message = new MailMessage();
    message.From = new MailAddress(from);
    message.Subject = subject;
    message.Body = body;
    message.To.Add(new MailAddress(to));
    await emailClient.SendMailAsync(message);
    logger.LogWarning("Sending email to {to} from {from} with subject {subject} using {type}.", to, from, subject, this.ToString());
  }
  
  public override string ToString()
  {
    return this.GetType().Name;
  }

 
}
