using Leve.Configurations;
using Leve.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Leve.Services;

public class EmailService
{
    private readonly EmailSettings _emailSettings;

    public EmailService(IOptions<EmailSettings> emailOptions)
    {
        _emailSettings = emailOptions.Value;
    }

    public async Task EnviarAsync(string destinatario, string assunto, string corpoHtml)
    {
        var mensagem = new MimeMessage();

        mensagem.From.Add(new MailboxAddress(_emailSettings.NomeRemetente, _emailSettings.EmailRemetente));
        mensagem.To.Add(MailboxAddress.Parse(destinatario));
        mensagem.Subject = assunto;

        mensagem.Body = new BodyBuilder
        {
            HtmlBody = corpoHtml
        }.ToMessageBody();

        using var smtp = new SmtpClient();

        var secureSocketOption = _emailSettings.UsarSsl
            ? SecureSocketOptions.SslOnConnect
            : SecureSocketOptions.StartTlsWhenAvailable;

        await smtp.ConnectAsync(_emailSettings.Host, _emailSettings.Port, secureSocketOption);

        if (!string.IsNullOrWhiteSpace(_emailSettings.Usuario))
        {
            await smtp.AuthenticateAsync(_emailSettings.Usuario, _emailSettings.Senha);
        }

        await smtp.SendAsync(mensagem);
        await smtp.DisconnectAsync(true);
    }
}