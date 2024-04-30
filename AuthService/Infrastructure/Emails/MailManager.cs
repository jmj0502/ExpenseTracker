using Microsoft.Extensions.Options;
using MimeKit;

namespace AuthService.Infrastructure.Emails;

public class MailManager
{
    private readonly ILogger<MailManager> _logger;
    private readonly MailSettings _mailSettings;
    public MailManager(ILogger<MailManager> logger, IOptions<MailSettings> mailSettings)
    {
        _logger = logger;
        _mailSettings = mailSettings.Value;
    }

    public async Task<bool> SendMailAsync(MailData mailData)
    {
        try
        {
            using var emailMessage = new MimeMessage();
            var mailboxAddress = new MailboxAddress(
                _mailSettings.SenderName, _mailSettings.SenderEmail);
            emailMessage.From.Add(mailboxAddress);
            var mailTo = new MailboxAddress(
                mailData.RecipientName, mailData.RecipientAddress);
            emailMessage.To.Add(mailTo);
            emailMessage.Subject = mailData.Subject;
            var mailBodyBuilder = new BodyBuilder();
            mailBodyBuilder.HtmlBody = mailData.Body;

            emailMessage.Body = mailBodyBuilder.ToMessageBody();
            using var mailClient = new MailKit.Net.Smtp.SmtpClient();
            var USE_SSL = false;
            var QUIT = false;
            await mailClient.ConnectAsync(
                _mailSettings.Server, _mailSettings.Port, USE_SSL);
            mailClient.Authenticate(_mailSettings.UserName, _mailSettings.Password);
            await mailClient.SendAsync(emailMessage);
            await mailClient.DisconnectAsync(QUIT);
            _logger.LogInformation($"[MailManager-SendMail]: Mail sent!");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"[MailManager-SendMail/Error]: {ex}");
            return false;
        }
    }
}
