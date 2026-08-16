using Clean.Architecture.Core.Interfaces;

namespace Clean.Architecture.Infrastructure.Email;

public class MimeKitEmailSender(ILogger<MimeKitEmailSender> logger,
  IOptions<MailserverConfiguration> mailserverOptions) : IEmailSender
{
  private readonly ILogger<MimeKitEmailSender> _logger = logger;
  private readonly MailserverConfiguration _mailserverConfiguration = mailserverOptions.Value!;

  public async Task SendEmailAsync(string recipientEmail, string senderEmail, string subject, string body)
  {
    _logger.LogWarning("Sending email with subject {Subject} using {Type}.", subject, this.ToString());

    using var client = new MailKit.Net.Smtp.SmtpClient();
    await client.ConnectAsync(_mailserverConfiguration.Hostname,
      _mailserverConfiguration.Port, false);
    var message = new MimeMessage();
    message.From.Add(new MailboxAddress(senderEmail, senderEmail));
    message.To.Add(new MailboxAddress(recipientEmail, recipientEmail));
    message.Subject = subject;
    message.Body = new TextPart("plain") { Text = body };

    await client.SendAsync(message);

    await client.DisconnectAsync(true,
      new CancellationToken(canceled: true));
  }
}
