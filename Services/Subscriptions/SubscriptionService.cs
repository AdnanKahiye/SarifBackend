using AutoMapper;
using AutoMapper.QueryableExtensions;
using Backend.DTOs.Requests.Setup;
using Backend.DTOs.Requests.Subscriptions;
using Backend.DTOs.Responses.Setup;
using Backend.DTOs.Responses.Subscriptions;
using Backend.Interfaces;
using Backend.Interfaces.Subscription;
using Backend.Models.Setup;
using Backend.Models.Subscription;
using Backend.Persistence;
using Backend.Utiliy;
using Backend.Wrapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Backend.Services.Subscriptions
{
    public class SubscriptionService : CacheService, ISubscriptionService
    {

        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUser;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private const string PlanCacheKey = "APlanCache";
        private const string BranchCacheKey = "BranchCache";

       public SubscriptionService( AppDbContext context,IMapper mapper,IMemoryCache cache,ICurrentUserService currentUser,IHttpContextAccessor httpContextAccessor
        ) : base(cache)
                {
                    _context = context;
                    _mapper = mapper;
                    _currentUser = currentUser;
                    _httpContextAccessor = httpContextAccessor;
                }


        // CREATE
        // ================================
        public async Task<ResponseWrapper<Guid>> CreatePlanAsync(CreatePlanDto dto)
        {
            if (string.IsNullOrEmpty(_currentUser.UserId))
            {
                return await ResponseWrapper<Guid>.FailureAsync("Unauthorized", "User not authenticated", 401);
            }

            return await ExecuteWriteAsync(async () =>
            {
                try
                {
                    var entity = _mapper.Map<Plan>(dto);

                    entity.Id = Guid.NewGuid();
                    entity.UserId = _currentUser.UserId;
                    entity.CreatedAt = DateTime.UtcNow;
                    entity.UpdatedAt = DateTime.UtcNow;


                    _context.Plans.Add(entity);
                    await _context.SaveChangesAsync();

                    _cache.Remove(PlanCacheKey);

                    return entity.Id;
                }
                catch (DbUpdateException ex)
                {
                    var msg = ex.InnerException?.Message ?? ex.Message;
                    throw new Exception($"Database error while creating Plan: {msg}");
                }
                catch (Exception ex)
                {
                    throw new Exception($"Unexpected error while creating plan: {ex.Message}");
                }

            }, "Plan created successfully", "Error creating plan");
        }

        public async Task<ResponseWrapper<PagedResponse<PlanDto>>> GetAllPlansAsync(int page = 1, int pageSize = 10)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;
            pageSize = Math.Min(pageSize, 100);

            return await ExecuteWithCacheAsync(
                cacheKey: $"{PlanCacheKey}_{_currentUser.UserId}_{page}_{pageSize}",
                action: async () =>
                {
                    var query = _context.Plans
                        .Where(x => x.UserId == _currentUser.UserId) // 🔐 important
                        .AsNoTracking();

                    var totalRecords = await _context.Plans
                        .Where(x => x.UserId == _currentUser.UserId)
                        .CountAsync();

                    var mapped = await query
                        .OrderByDescending(x => x.CreatedAt)
                        .ThenBy(x => x.Id)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ProjectTo<PlanDto>(_mapper.ConfigurationProvider)
                        .ToListAsync();

                    return new PagedResponse<PlanDto>(mapped, page, pageSize, totalRecords);
                },
                successMessageFactory: result => $"{result.Data.Count} of {result.TotalRecords} plan fetched",
                cacheMessage: "Plan fetched from cache",
                errorMessage: "Error fetching plan"
            );
        }


        public async Task<ResponseWrapper<bool>> UpdatePlanAsync(Guid id, UpdatePlanDto dto)
        {
            return await ExecuteWriteAsync(async () =>
            {
                try
                {
                    var entity = await _context.Plans.FindAsync(id);

                    if (entity == null)
                        throw new Exception("Plan not found");

                    _mapper.Map(dto, entity);

                    entity.UpdatedAt = DateTime.UtcNow;

                    await _context.SaveChangesAsync();

                    _cache.Remove(PlanCacheKey);

                    return true;
                }
                catch (DbUpdateException ex)
                {
                    var msg = ex.InnerException?.Message ?? ex.Message;
                    throw new Exception($"Database error while updating plan: {msg}");
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error updating plan: {ex.Message}");
                }

            }, "Plan updated successfully", "Error updating plan");
        }

        // ================================
        // DELETE
        // ================================
        public async Task<ResponseWrapper<bool>> DeletePlanAsync(Guid id)
        {
            return await ExecuteWriteAsync(async () =>
            {
                try
                {
                    var entity = await _context.Plans.FindAsync(id);

                    if (entity == null)
                        throw new Exception("Plan not found");

                    _context.Plans.Remove(entity);
                    await _context.SaveChangesAsync();

                    _cache.Remove(PlanCacheKey);

                    return true;
                }
                catch (DbUpdateException ex)
                {
                    var msg = ex.InnerException?.Message ?? ex.Message;
                    throw new Exception($"Database error while deleting plan: {msg}");
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error deleting plan: {ex.Message}");
                }

            }, "Plan deleted successfully", "Error deleting plan");
        }





        // CREATE
        // ================================
        public async Task<ResponseWrapper<Guid>> CreateAssignPermissionPlanAsync(AssignPermissionsToPlanDto dto)
        {
            if (string.IsNullOrEmpty(_currentUser.UserId))
            {
                return await ResponseWrapper<Guid>.FailureAsync("Unauthorized", "User not authenticated", 401);
            }

            return await ExecuteWriteAsync(async () =>
            {
                try
                {
                    var entities = new List<PlanPermission>();

                    foreach (var permissionId in dto.PermissionIds)
                    {
                        entities.Add(new PlanPermission
                        {
                            Id = Guid.NewGuid(),
                            PlanId = dto.PlanId,
                            PermissionId = permissionId,
                            UserId = _currentUser.UserId,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        });
                    }

                    await _context.PlanPermissions.AddRangeAsync(entities);
                    await _context.SaveChangesAsync();

                    _cache.Remove(PlanCacheKey);

                    // return one id or just success
                    return dto.PlanId;
                }
                catch (DbUpdateException ex)
                {
                    var msg = ex.InnerException?.Message ?? ex.Message;
                    throw new Exception($"Database error while creating Plan Permissions: {msg}");
                }
                catch (Exception ex)
                {
                    throw new Exception($"Unexpected error while creating plan permissions: {ex.Message}");
                }

            }, "Plan Permissions created successfully", "Error creating plan permissions");
        }
        public async Task<ResponseWrapper<PagedResponse<PlanPermissionDto>>> GetAllPermissionPlanAsync(int page = 1, int pageSize = 10)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;
            pageSize = Math.Min(pageSize, 100);

            return await ExecuteWithCacheAsync(
                cacheKey: $"{PlanCacheKey}_{_currentUser.UserId}_{page}_{pageSize}",
                action: async () =>
                {
                    var query = _context.PlanPermissions
                        .Where(x => x.UserId == _currentUser.UserId)
                        .Include(x => x.Plan)
                        .Include(x => x.Permission)
                        .AsNoTracking();

                    var totalRecords = await query.CountAsync();

                    var data = await query
                        .OrderByDescending(x => x.CreatedAt)
                        .ThenBy(x => x.Id)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .Select(x => new PlanPermissionDto
                        {
                            Id = x.Id,
                            PlanId = x.PlanId,
                            PlanName = x.Plan.Name,

                            PermissionId = x.PermissionId,
                            PermissionName = x.Permission.Name,
                            PermissionKey = x.Permission.KeyName
                        })
                        .ToListAsync();

                    return new PagedResponse<PlanPermissionDto>(data, page, pageSize, totalRecords);
                },
                successMessageFactory: result => $"{result.Data.Count} of {result.TotalRecords} plan permissions fetched",
                cacheMessage: "Plan permissions fetched from cache",
                errorMessage: "Error fetching plan permissions"
            );
        }

        public async Task<ResponseWrapper<bool>> UpdatePermissionPlanAsync(Guid id, UpdatePermissionsToPlanDto dto)
        {
            return await ExecuteWriteAsync(async () =>
            {
                try
                {
                    if (id != dto.PlanId)
                        throw new Exception("PlanId mismatch");

                    // ✅ Check plan exists
                    var planExists = await _context.Plans
                        .AnyAsync(x => x.Id == dto.PlanId && x.UserId == _currentUser.UserId);

                    if (!planExists)
                        throw new Exception("Plan not found");

                    // ✅ Get existing permissions
                    var existingPermissions = await _context.PlanPermissions
                        .Where(x => x.PlanId == dto.PlanId && x.UserId == _currentUser.UserId)
                        .ToListAsync();

                    // ✅ Remove old permissions
                    _context.PlanPermissions.RemoveRange(existingPermissions);

                    // ✅ Add new permissions
                    var newPermissions = dto.PermissionIds.Select(permissionId => new PlanPermission
                    {
                        Id = Guid.NewGuid(),
                        PlanId = dto.PlanId,
                        PermissionId = permissionId,
                        UserId = _currentUser.UserId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }).ToList();

                    await _context.PlanPermissions.AddRangeAsync(newPermissions);

                    await _context.SaveChangesAsync();

                    _cache.Remove(PlanCacheKey);

                    return true;
                }
                catch (DbUpdateException ex)
                {
                    var msg = ex.InnerException?.Message ?? ex.Message;
                    throw new Exception($"Database error while updating plan permissions: {msg}");
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error updating plan permissions: {ex.Message}");
                }

            }, "Plan permissions updated successfully", "Error updating plan permissions");
        }
        // ================================
        // DELETE
        // ================================
        public async Task<ResponseWrapper<bool>> DeletePermissionPlanAsync(Guid id)
        {
            return await ExecuteWriteAsync(async () =>
            {
                try
                {
                    var entity = await _context.PlanPermissions
                        .FirstOrDefaultAsync(x => x.Id == id && x.UserId == _currentUser.UserId);

                    if (entity == null)
                        throw new Exception("Plan permission not found");

                    _context.PlanPermissions.Remove(entity);

                    await _context.SaveChangesAsync();

                    _cache.Remove(PlanCacheKey);

                    return true;
                }
                catch (DbUpdateException ex)
                {
                    var msg = ex.InnerException?.Message ?? ex.Message;
                    throw new Exception($"Database error while deleting plan permission: {msg}");
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error deleting plan permission: {ex.Message}");
                }

            }, "Permission removed from plan successfully", "Error removing permission from plan");
        }









        // CREATE
        // ================================
        public async Task<ResponseWrapper<Guid>> CreateSubscriptionAsync(CreateSubscriptionDto dto)
        {
            if (string.IsNullOrEmpty(_currentUser.UserId))
            {
                return await ResponseWrapper<Guid>.FailureAsync("Unauthorized", "User not authenticated", 401);
            }

            return await ExecuteWriteAsync(async () =>
            {
                try
                {
                    var entity = _mapper.Map<Models.Subscription.Subscriptions>(dto);

                    entity.Id = Guid.NewGuid();
                    entity.UserId = _currentUser.UserId;

                    // SAXID: Hubi in taariikhaha DTO-ga laga soo qaatay loo beddelo UTC Kind
                    entity.StartDate = DateTime.SpecifyKind(dto.StartDate, DateTimeKind.Utc);
                    entity.EndDate = DateTime.SpecifyKind(dto.EndDate, DateTimeKind.Utc);

                    entity.CreatedAt = DateTime.UtcNow;
                    entity.UpdatedAt = DateTime.UtcNow;

                    _context.Subscriptions.Add(entity);
                    await _context.SaveChangesAsync();

                    _cache.Remove(PlanCacheKey);

                    return entity.Id;
                }
                catch (DbUpdateException ex)
                {
                    // InnerException-ka Postgre wuxuu ku siinayaa faahfaahinta dhabta ah
                    var msg = ex.InnerException?.Message ?? ex.Message;
                    throw new Exception($"Database error while creating subscription: {msg}");
                }
                catch (Exception ex)
                {
                    throw new Exception($"Unexpected error while creating subscription: {ex.Message}");
                }

            }, "Subscription created successfully", "Error creating subscription");
        }

        public async Task<ResponseWrapper<PagedResponse<SubscriptionDto>>> GetAllSubscriptionAsync(int page = 1, int pageSize = 10)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;
            pageSize = Math.Min(pageSize, 100);

            return await ExecuteWithCacheAsync(
                cacheKey: $"{PlanCacheKey}_{_currentUser.UserId}_{page}_{pageSize}",
                action: async () =>
                {
                    var query = _context.Subscriptions
                        .Where(x => x.UserId == _currentUser.UserId) // 🔐 important
                        .Include(x => x.Agency)
                        .Include(x => x.Plan)
                        .AsNoTracking();

                    var totalRecords = await query.CountAsync();

                    var data = await query
                        .OrderByDescending(x => x.StartDate)
                        .ThenBy(x => x.Id)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .Select(x => new SubscriptionDto
                        {
                            AgencyId = x.AgencyId,
                            AgencyName = x.Agency.Name,

                            PlanId = x.PlanId,
                            PlanName = x.Plan.Name,

                            Status = (int)x.Status,
                            StartDate = x.StartDate,
                            EndDate = x.EndDate
                        })
                        .ToListAsync();

                    return new PagedResponse<SubscriptionDto>(data, page, pageSize, totalRecords);
                },
                successMessageFactory: result => $"{result.Data.Count} of {result.TotalRecords} subscriptions fetched",
                cacheMessage: "Subscriptions fetched from cache",
                errorMessage: "Error fetching subscriptions"
            );
        }

        public async Task<ResponseWrapper<bool>> UpdateSubscriptionAsync(Guid id, UpdateSubscriptionDto dto)
        {
            return await ExecuteWriteAsync(async () =>
            {
                try
                {
                    var entity = await _context.Subscriptions
                        .FirstOrDefaultAsync(x => x.Id == id && x.UserId == _currentUser.UserId);

                    if (entity == null)
                        throw new Exception("Subscription not found");

                    // ✅ Manual mapping (recommended)
                    entity.AgencyId = dto.AgencyId;
                    entity.PlanId = dto.PlanId;
                    entity.Status = (SubscriptionStatus)dto.Status;
                    entity.StartDate = dto.StartDate;
                    entity.EndDate = dto.EndDate;

                    entity.UpdatedAt = DateTime.UtcNow;

                    await _context.SaveChangesAsync();

                    _cache.Remove(PlanCacheKey);

                    return true;
                }
                catch (DbUpdateException ex)
                {
                    var msg = ex.InnerException?.Message ?? ex.Message;
                    throw new Exception($"Database error while updating subscription: {msg}");
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error updating subscription: {ex.Message}");
                }

            }, "Subscription updated successfully", "Error updating subscription");
        }
        // ================================
        // DELETE
        // ================================
        public async Task<ResponseWrapper<bool>> DeleteSubscriptionAsync(Guid id)
        {
            return await ExecuteWriteAsync(async () =>
            {
                try
                {
                    var entity = await _context.Subscriptions.FindAsync(id);

                    if (entity == null)
                        throw new Exception("Subscription not found");

                    _context.Subscriptions.Remove(entity);
                    await _context.SaveChangesAsync();

                    _cache.Remove(PlanCacheKey);

                    return true;
                }
                catch (DbUpdateException ex)
                {
                    var msg = ex.InnerException?.Message ?? ex.Message;
                    throw new Exception($"Database error while deleting subscription: {msg}");
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error deleting subscription: {ex.Message}");
                }

            }, "Subscription deleted successfully", "Error deleting subscription");
        }


    }
}
