using Resend;

namespace SpeakUp.API.Services;

public class EmailService
{
    private readonly ResendClient _resend;
    private readonly IConfiguration _configuration;


    public EmailService(
        ResendClient resend,
        IConfiguration configuration)
    {
        _resend = resend;
        _configuration = configuration;
    }


    public async Task SendVerificationEmail(
        string email,
        string code)
    {

        var fromEmail =
            _configuration["Resend:FromEmail"];


        if (string.IsNullOrEmpty(fromEmail))
        {
            throw new Exception(
                "Resend FromEmail missing."
            );
        }


        var message = new EmailMessage
        {
            From = fromEmail,

            To = email,

            Subject =
            "Your SpeakUp verification code",


            HtmlBody =
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

            <br/>

            <p>
            SpeakUp Team
            </p>

            </body>
            </html>
            "
        };


        await _resend.EmailSendAsync(message);
    }

    public async Task SendPasswordResetEmail(
    string email,
    string token)
    {
        var fromEmail =
            _configuration["Resend:FromEmail"];


        if (string.IsNullOrEmpty(fromEmail))
        {
            throw new Exception(
                "Resend FromEmail missing."
            );
        }


        var frontendUrl =
            _configuration["FrontendUrl"];


        var resetLink =
            $"{frontendUrl}/reset-password?email={email}&token={token}";


        var message = new EmailMessage
        {
            From = fromEmail,

            To = email,

            Subject =
            "Reset your SpeakUp password",


            HtmlBody =
            $@"
        <html>
        <body>

        <h2>SpeakUp Password Reset</h2>

        <p>
        We received a request to reset your password.
        </p>

        <p>
        Click the link below to create a new password:
        </p>


        <a href='{resetLink}'
        style='
        display:inline-block;
        padding:12px 20px;
        background:#2563eb;
        color:white;
        text-decoration:none;
        border-radius:8px;
        '>
        Reset Password
        </a>


        <p>
        This link expires in 15 minutes.
        </p>


        <p>
        If you did not request this, ignore this email.
        </p>


        <br/>

        <p>
        SpeakUp Team
        </p>


        </body>
        </html>
        "
        };


        await _resend.EmailSendAsync(message);
    }

    public async Task SendAdminInvitationEmail(
    string email,
    string role,
    string signupLink)
    {
        var fromEmail =
            _configuration["Resend:FromEmail"];


        if (string.IsNullOrEmpty(fromEmail))
        {
            throw new Exception(
                "Resend FromEmail missing."
            );
        }



        var message = new EmailMessage
        {
            From = fromEmail,

            To = email,

            Subject =
            "You have been invited to join SpeakUp Admin Portal",


            HtmlBody =
            $@"
        <html>
        <body>

        <h2>Welcome to SpeakUp</h2>


        <p>
        You have been invited to create a 
        <strong>{role}</strong> account on the SpeakUp platform.
        </p>


        <p>
        SpeakUp is a student support and reporting system 
        designed to help manage student safety and concerns.
        </p>


        <p>
        Click the button below to complete your account setup:
        </p>


        <a href='{signupLink}'
        style='
        display:inline-block;
        padding:12px 20px;
        background:#2563eb;
        color:white;
        text-decoration:none;
        border-radius:8px;
        font-weight:bold;
        '>
        Create Admin Account
        </a>


        <p>
        This invitation link can only be used once.
        </p>


        <p>
        The link expires in 24 hours.
        </p>


        <p>
        If you were not expecting this invitation, 
        you can safely ignore this email.
        </p>


        <br/>


        <p>
        SpeakUp Team
        </p>


        </body>
        </html>
        "
        };


        await _resend.EmailSendAsync(message);
    }
}