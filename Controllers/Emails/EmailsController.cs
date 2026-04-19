using Backend.DTOs.Requests.Emails;
using Backend.Interfaces.Emails;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.Emails
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailsController : ControllerBase
    {

        private readonly IOtpService _otpService;

        public EmailsController(IOtpService otpService)
        {
            _otpService = otpService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendOtp([FromBody] SendOtpRequest request)
        {
            await _otpService.SendOtpAsync(request.Email, request.Purpose);

            return Ok(new
            {
                success = true,
                message = "OTP sent successfully."
            });
        }


        [HttpPost("verify")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            var isValid = await _otpService.VerifyOtpAsync(
                request.Email,
                request.Code,
                request.Purpose);

            if (!isValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid or expired OTP."
                });
            }

            return Ok(new
            {
                success = true,
                message = "OTP verified successfully."
            });
        }

    }
}
