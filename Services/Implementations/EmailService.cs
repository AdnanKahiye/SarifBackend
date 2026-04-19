using Backend.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Backend.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly string _host;
        private readonly int _port;
        private readonly string _username;
        private readonly string _password;
        private readonly string _fromName;
        private readonly string _fromEmail; // New field

        public EmailService(
            string host,
            int port,
            string username,
            string password,
            string fromName,
            string fromEmail) // Injecting the sender email
        {
            _host = host;
            _port = port;
            _username = username;
            _password = password;
            _fromName = fromName;
            _fromEmail = fromEmail;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var email = new MimeMessage();

            // Use the injected configuration instead of hard-coding
            email.From.Add(new MailboxAddress(_fromName, _fromEmail));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;
            email.Body = new TextPart("html") { Text = body };

            using var smtp = new SmtpClient();
            try
            {
                await smtp.ConnectAsync(_host, _port, SecureSocketOptions.StartTls);

                // Still authenticating with the Brevo ID
                await smtp.AuthenticateAsync(_username, _password);

                await smtp.SendAsync(email);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                await smtp.DisconnectAsync(true);
            }
        }
    }
}