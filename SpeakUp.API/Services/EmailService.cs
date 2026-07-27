using System.Net;
using System.Net.Mail;

namespace SpeakUp.API.Services;

public class EmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }


    public async Task SendVerificationEmail(
        string email,
        string code)
    {
        var settings =
            _configuration.GetSection("EmailSettings");


        var senderEmail = settings["Email"];
        var password = settings["Password"];
        var host = settings["Host"];
        var portString = settings["Port"];


        if (string.IsNullOrEmpty(senderEmail) ||
            string.IsNullOrEmpty(password) ||
            string.IsNullOrEmpty(host) ||
            string.IsNullOrEmpty(portString))
        {
            throw new Exception(
                "Email settings are missing in appsettings.json"
            );
        }


        using var smtp = new SmtpClient
        {
            Host = host,
            Port = int.Parse(portString),
            EnableSsl = true,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(
                senderEmail,
                password
            )
        };

        using var mail = new MailMessage
        {
            From = new MailAddress(
                senderEmail,
                "SpeakUp Support"
            ),

            Subject = "Your SpeakUp verification code",

            Body = 
            $@"
            <html>
            <body>

            <h2>Welcome to SpeakUp</h2>

            <p>
            Thank you for creating your account.
            </p>

            <p>
            Your verification code is:
            </p>

            <h1>{code}</h1>

            <p>
            This code expires in 15 minutes.
            </p>

            <p>
            If you did not create this account, ignore this email.
            </p>

            <br>

            <p>
            SpeakUp Team
            </p>

            </body>
            </html>
            ",

            IsBodyHtml = true
        };

        mail.To.Add(email);

        await smtp.SendMailAsync(mail);
        await smtp.SendMailAsync(mail);
       
    }
}