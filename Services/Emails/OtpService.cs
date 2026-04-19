using Backend.Interfaces;
using Backend.Interfaces.Emails;
using Backend.Models.Emails;
using Backend.Persistence;
using Hangfire;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Emails
{
    public class OtpService : IOtpService
    {
        private readonly AppDbContext _context;

        public OtpService(AppDbContext context)
        {
            _context = context;
        }

        public async Task SendOtpAsync(string email, string purpose)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is required.");

            var existingOtps = await _context.OtpCodes
                .Where(x => x.Email == email && x.Purpose == purpose && !x.IsUsed)
                .ToListAsync();

            if (existingOtps.Any())
            {
                _context.OtpCodes.RemoveRange(existingOtps);
            }

            var otp = GenerateOtp();

            var entity = new OtpCode
            {
                Id = Guid.NewGuid(),
                Email = email,
                Code = otp,
                Purpose = purpose,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.OtpCodes.Add(entity);
            await _context.SaveChangesAsync();

            var subject = "Verification Code: " + otp;
            var body = $@"
    <div style=""background-color: #f6f9fc; padding: 40px 0; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6;"">
        <div style=""max-width: 500px; margin: 0 auto; background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);"">
            <div style=""background-color: #007bff; padding: 20px; text-align: center;"">
                <h2 style=""color: #ffffff; margin: 0; font-size: 24px;"">Kahiye App</h2>
            </div>
        
        <div style=""padding: 30px; text-align: center;"">
            <h3 style=""color: #333; margin-top: 0;"">Confirm Your Identity</h3>
            <p style=""color: #666; font-size: 16px;"">Please use the verification code below to complete your request. This code is valid for <strong>5 minutes</strong>.</p>
            
            <div style=""margin: 30px 0; padding: 20px; background-color: #f1f4f8; border-radius: 4px; border: 1px dashed #007bff;"">
                <span style=""font-size: 32px; font-weight: bold; letter-spacing: 6px; color: #007bff;"">{otp}</span>
            </div>
            
            <p style=""color: #999; font-size: 13px;"">If you did not request this code, you can safely ignore this email.</p>
        </div>
        
        <div style=""padding: 20px; background-color: #f9fafb; text-align: center; border-top: 1px solid #eeeeee;"">
            <p style=""color: #aaa; font-size: 12px; margin: 0;"">
                &copy; {DateTime.UtcNow.Year} Kahiye App. All rights reserved.
            </p>
        </div>
    </div>
</div>";

            BackgroundJob.Enqueue<IEmailService>(x =>
                x.SendEmailAsync(email, subject, body));
        }

        public async Task<bool> VerifyOtpAsync(string email, string code, string purpose)
        {
            var otp = await _context.OtpCodes
                .Where(x =>
                    x.Email == email &&
                    x.Code == code &&
                    x.Purpose == purpose &&
                    !x.IsUsed)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (otp == null)
                return false;

            if (otp.ExpiresAt < DateTime.UtcNow)
                return false;

            otp.IsUsed = true;
            await _context.SaveChangesAsync();

            return true;
        }

        private static string GenerateOtp()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }
    }
}
