using Clean.Architecture.Core.Interfaces;

namespace Clean.Architecture.Infrastructure.Email;

public class FakeEmailSender(ILogger<FakeEmailSender> logger) : IEmailSender
{
  private readonly ILogger<FakeEmailSender> _logger = logger;
  public Task SendEmailAsync(string recipientEmail, string senderEmail, string subject, string body)
  {
    _logger.LogInformation("Not actually sending an email with subject {Subject}", subject);
    return Task.CompletedTask;
  }
}
