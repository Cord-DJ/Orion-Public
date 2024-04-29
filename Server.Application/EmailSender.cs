using Cord.Server.Domain;
using FluentEmail.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace Cord.Server.Application;

public class EmailSender : IEmailSender {
    readonly IFluentEmailFactory emailFactory;

    public EmailSender(IFluentEmailFactory emailFactory) {
        this.emailFactory = emailFactory;
    }

    public async Task SendEmail(string recipient, EmailType type, object values) {
        var email = emailFactory.Create()
            .To(recipient)
            .Subject(GetSubject(type))
            .Tag(type.ToString())
            .UsingTemplateFromEmbedded(
                $"Cord.Server.Application.EmailTemplates.{GetTemplate(type)}.cshtml",
                values,
                typeof(EmailSender).Assembly
            );

        var result = await email.SendAsync();
        if (!result.Successful) {
            Log.Error("Unable to send email: {Message}", result.ErrorMessages);
        }
    }

    string GetSubject(EmailType type) =>
        type switch {
            EmailType.EmailVerification => "Verify Email Address on Cord.DJ",
            EmailType.EmailChange => "Change your Email Address on Cord.DJ",
            EmailType.PasswordRecovery => "Password Reset Request on Cord.DJ",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

    string GetTemplate(EmailType type) =>
        type switch {
            EmailType.EmailVerification => "VerifyEmail",
            EmailType.EmailChange => "EmailChange",
            EmailType.PasswordRecovery => "PasswordRecovery",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
}

public static class EmailSenderExtensions {
    public static void InitEmailSender(this WebApplicationBuilder builder) {
        var emailOptions = builder.Configuration.GetSection(EmailOptions.Section).Get<EmailOptions>();
        var sendGridOptions = builder.Configuration.GetSection(SendgridOptions.Section).Get<SendgridOptions>();

        builder.Services
            .AddFluentEmail(emailOptions.Sender, emailOptions.Name)
            .AddRazorRenderer(typeof(EmailSender))
            .AddSendGridSender(sendGridOptions.ApiKey);

        builder.Services.AddTransient<IEmailSender, EmailSender>();
    }
}
