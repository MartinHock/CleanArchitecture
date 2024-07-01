﻿using Clean.Architecture.Core.Interfaces;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Clean.Architecture.Infrastructure.Email;

public class MimeKitEmailSender(
  ILogger<MimeKitEmailSender> logger,
  IOptions<MailserverConfiguration> mailserverOptions)
  : IEmailSender
{
  private readonly MailserverConfiguration _mailserverConfiguration = mailserverOptions.Value!;


  public async Task SendEmailAsync(string to, string from, string subject, string body)
  {
    logger.LogWarning("Sending email to {to} from {from} with subject {subject} using {type}.", to, from, subject, this.ToString());

    using var client = new SmtpClient(); 
    client.Connect(_mailserverConfiguration.Hostname, 
      _mailserverConfiguration.Port, false);
    using var message = new MimeMessage();
    message.From.Add(new MailboxAddress(from, from));
    message.To.Add(new MailboxAddress(to, to));
    message.Subject = subject;
    message.Body = new TextPart("plain") { Text = body };

    await client.SendAsync(message);

    await client.DisconnectAsync(true, 
      new CancellationToken(canceled: true));
  }
  public override string ToString()
  {
    return this.GetType().Name;
  }
}
