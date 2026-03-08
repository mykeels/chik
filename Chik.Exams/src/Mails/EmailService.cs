using Microsoft.Extensions.DependencyInjection;
using System.Net.Mail;
using System.Net;
using System;
using Chik.Exams.Mails;

namespace Chik.Exams;

public record EmailCredentials(string Password);

public interface IEmailService
{
    Task<bool> SendEmail<TData>(string to, IEmailTemplate<TData> template, TData? data = null) where TData : class;
    Task<bool> SendEmail(string to, string subject, string body);
}

public class EmailService : IEmailService
{
    public const string AdminEmail = "admin@mykeels.com";
    private readonly EmailCredentials _credentials;
    private readonly ILogger<EmailService> _logger;

    public EmailService(EmailCredentials credentials, ILogger<EmailService> logger)
    {
        _credentials = credentials;
        _logger = logger;
    }

    public async Task<bool> SendEmail<TData>(string to, IEmailTemplate<TData> template, TData? data = null) where TData : class
    {
        var html = await template.WithData(data ?? template.Data).GetHtml();
        return await SendEmail(to, template.GetSubject(), html);
    }

    public async Task<bool> SendEmail(string to, string subject, string body)
    {
        try
        {
            // Validate email format
            if (!SesEmailHelper.IsValidEmailFormat(to))
            {
                Console.WriteLine($"ERROR: Invalid email format: {to}");
                return false;
            }

            // Check if email is likely verified (for guidance)
            if (!SesEmailHelper.IsLikelyVerifiedEmail(to))
            {
                Console.WriteLine($"WARNING: Email {to} may not be verified in AWS SES.");
                Console.WriteLine("If you get a verification error, you'll need to verify this email address.");
            }

            var mail = new MailMessage();
            mail.From = new MailAddress("<support@under4.games>", "Under 4 Games");
            mail.To.Add(to);
            mail.Bcc.Add("user@under4.games");
            mail.ReplyToList.Add("support@under4.games");
            mail.Subject = subject;
            mail.Body = body;
            mail.IsBodyHtml = true;

            // Get AWS SES SMTP settings
            var (smtpHost, smtpPort) = AwsSesConfiguration.GetSmtpSettings("us-east-1", false);
            
            var client = new SmtpClient(smtpHost, smtpPort);
            client.Credentials = new NetworkCredential("AKIATZA52X5CLC4QX5NY", _credentials.Password);
            client.EnableSsl = true;
            client.UseDefaultCredentials = false;

            if (DryRun.IsLive)
            {   
                await client.SendMailAsync(mail);
                _logger.Info($"Sent email to {to}", new { to, subject });
                return true;
            }
            
            _logger.Info($"Dry run: Would have sent email to {to}", new { to, subject });
            return false;
        }
        catch (SmtpException ex)
        {
            // Handle specific SMTP errors
            _logger.Error(ex, $"SMTP Error: {ex.Message}");
            
            if (ex.Message.Contains("Email address is not verified"))
            {
                _logger.Error(ex, "ERROR: The recipient email address is not verified in AWS SES.");
                SesEmailHelper.PrintVerificationGuidance(to);
            }
            else if (ex.Message.Contains("Message rejected"))
            {
                _logger.Error(ex, "ERROR: Message was rejected by AWS SES.");
                _logger.Error(ex, "Check your AWS SES configuration and credentials.");
            }
            
            return false;
        }
        catch (Exception ex)
        {
            // Log other exceptions
            _logger.Error(ex, $"Failed to send email: {ex.Message}");
            if (ex.InnerException != null)
            {
                _logger.Error(ex, $"Inner exception: {ex.InnerException.Message}");
            }
            return false;
        }
    }
}
