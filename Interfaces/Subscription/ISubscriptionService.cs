using Backend.DTOs.Requests.Setup;
using Backend.DTOs.Requests.Subscriptions;
using Backend.DTOs.Responses.Setup;
using Backend.DTOs.Responses.Subscriptions;
using Backend.Wrapper;

namespace Backend.Interfaces.Subscription
{
    public interface ISubscriptionService
    {
        /// <summary>
        /// Plan 
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        Task<ResponseWrapper<Guid>> CreatePlanAsync(CreatePlanDto dto);

        Task<ResponseWrapper<PagedResponse<PlanDto>>> GetAllPlansAsync(int page = 1, int pageSize = 10);

        Task<ResponseWrapper<bool>> UpdatePlanAsync(Guid id, UpdatePlanDto dto);

        Task<ResponseWrapper<bool>> DeletePlanAsync(Guid id);





        /// <summary>
        /// Permission Plan 
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        Task<ResponseWrapper<Guid>> CreateAssignPermissionPlanAsync(AssignPermissionsToPlanDto dto);

        Task<ResponseWrapper<PagedResponse<PlanPermissionDto>>> GetAllPermissionPlanAsync(int page = 1, int pageSize = 10);

        Task<ResponseWrapper<bool>> UpdatePermissionPlanAsync(Guid id, UpdatePermissionsToPlanDto dto);

        Task<ResponseWrapper<bool>> DeletePermissionPlanAsync(Guid id);






        /// <summary>
        /// Permission Plan 
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        Task<ResponseWrapper<Guid>> CreateSubscriptionAsync(CreateSubscriptionDto dto);

        Task<ResponseWrapper<PagedResponse<SubscriptionDto>>> GetAllSubscriptionAsync(int page = 1, int pageSize = 10);

        Task<ResponseWrapper<bool>> UpdateSubscriptionAsync(Guid id, UpdateSubscriptionDto dto);

        Task<ResponseWrapper<bool>> DeleteSubscriptionAsync(Guid id);



    }
}
