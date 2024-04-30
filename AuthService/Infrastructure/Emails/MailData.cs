namespace AuthService.Infrastructure.Emails;

public record MailData(
    string RecipientAddress, string RecipientName, string Subject, string Body);

