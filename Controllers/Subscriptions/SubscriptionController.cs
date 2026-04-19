using Backend.DTOs.Requests.Subscriptions;
using Backend.Interfaces.Subscription;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.Subscriptions
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SubscriptionController : ControllerBase
    {
        private readonly ISubscriptionService _service;

        public SubscriptionController(ISubscriptionService service)
        {
            _service = service;
        }

        // ================================
        // PLAN ENDPOINTS
        // ================================

        [HttpPost("plan")]
        public async Task<IActionResult> CreatePlan([FromBody] CreatePlanDto dto)
        {
            var result = await _service.CreatePlanAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("plans")]
        public async Task<IActionResult> GetPlans(int page = 1, int pageSize = 10)
        {
            var result = await _service.GetAllPlansAsync(page, pageSize);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("plan/{id}")]
        public async Task<IActionResult> UpdatePlan(Guid id, [FromBody] UpdatePlanDto dto)
        {
            var result = await _service.UpdatePlanAsync(id, dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("plan/{id}")]
        public async Task<IActionResult> DeletePlan(Guid id)
        {
            var result = await _service.DeletePlanAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        // ================================
        // PLAN PERMISSIONS
        // ================================

        [HttpPost("plan/permissions")]
        public async Task<IActionResult> AssignPermissions([FromBody] AssignPermissionsToPlanDto dto)
        {
            var result = await _service.CreateAssignPermissionPlanAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("plan/permissions")]
        public async Task<IActionResult> GetPlanPermissions(int page = 1, int pageSize = 10)
        {
            var result = await _service.GetAllPermissionPlanAsync(page, pageSize);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("plan/permissions/{planId}")]
        public async Task<IActionResult> UpdatePlanPermissions(Guid planId, [FromBody] UpdatePermissionsToPlanDto dto)
        {
            var result = await _service.UpdatePermissionPlanAsync(planId, dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("plan/permissions/{id}")]
        public async Task<IActionResult> DeletePlanPermission(Guid id)
        {
            var result = await _service.DeletePermissionPlanAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        // ================================
        // SUBSCRIPTIONS
        // ================================

        [HttpPost("subscription")]
        public async Task<IActionResult> CreateSubscription([FromBody] CreateSubscriptionDto dto)
        {
            var result = await _service.CreateSubscriptionAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("subscriptions")]
        public async Task<IActionResult> GetSubscriptions(int page = 1, int pageSize = 10)
        {
            var result = await _service.GetAllSubscriptionAsync(page, pageSize);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("subscription/{id}")]
        public async Task<IActionResult> UpdateSubscription(Guid id, [FromBody] UpdateSubscriptionDto dto)
        {
            var result = await _service.UpdateSubscriptionAsync(id, dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("subscription/{id}")]
        public async Task<IActionResult> DeleteSubscription(Guid id)
        {
            var result = await _service.DeleteSubscriptionAsync(id);
            return StatusCode(result.StatusCode, result);
        }
    }
}