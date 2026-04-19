using Backend.DTOs.Requests.Customers;
using Backend.DTOs.Requests.Setup;
using Backend.Interfaces.Customers;
using Backend.Interfaces.Setup;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.Customers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CustomerController : ControllerBase
    {

        private readonly ICustomersService _customerService;

        public CustomerController(ICustomersService customerService)
        {
            _customerService = customerService;
        }




        // ============================
        // Customers
        // ============================

        [HttpPost("customer")]
        public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerDto dto)
        {
            var result = await _customerService.CreateCustomerAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("customer")]
        public async Task<IActionResult> GetCustomers(int page = 1, int pageSize = 10)
        {
            var result = await _customerService.GetAllCustomerAsync(page, pageSize);
            return Ok(result);
        }

        [HttpPut("customer/{id}")]
        public async Task<IActionResult> UpdateAgency(Guid id, [FromBody] UpdateCustomerDto dto)
        {
            var result = await _customerService.UpdateCustomerAsync(id, dto);
            return Ok(result);
        }

        [HttpDelete("customer/{id}")]
        public async Task<IActionResult> DeleteAgency(Guid id)
        {
            var result = await _customerService.DeleteCustomerAsync(id);
            return Ok(result);
        }
    }
}
