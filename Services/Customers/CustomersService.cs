using AutoMapper;
using AutoMapper.QueryableExtensions;
using Backend.DTOs.Requests.Customers;
using Backend.DTOs.Requests.Setup;
using Backend.DTOs.Responses.Customers;
using Backend.DTOs.Responses.Setup;
using Backend.Interfaces;
using Backend.Interfaces.Customers;
using Backend.Models.Setup;
using Backend.Persistence;
using Backend.Utiliy;
using Backend.Wrapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Backend.Services.Customers
{
    public class CustomersService : CacheService, ICustomersService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUser;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private const string CustomerCacheKey = "CustomerCache";

        public CustomersService(
                            AppDbContext context,
                            IMapper mapper,
                            IMemoryCache cache,
                            ICurrentUserService currentUser,
                            IHttpContextAccessor httpContextAccessor
                        ) : base(cache)
                                {
            _context = context;
            _mapper = mapper;
            _currentUser = currentUser;
            _httpContextAccessor = httpContextAccessor;
        }



        // ================================
        // CREATE
        // ================================
        public async Task<ResponseWrapper<Guid>> CreateCustomerAsync(CreateCustomerDto dto)
        {
            if (string.IsNullOrEmpty(_currentUser.UserId))
            {
                return await ResponseWrapper<Guid>.FailureAsync("Unauthorized", "User not authenticated", 401);
            }

            return await ExecuteWriteAsync(async () =>
            {
                try
                {
                    var entity = _mapper.Map<Models.Customers.Customer>(dto);

                    entity.Id = Guid.NewGuid();
                    entity.UserId = _currentUser.UserId;
                    entity.AgencyId = _currentUser.AgencyId;
                    entity.BranchId = _currentUser.BranchId;
                    entity.CreatedAt = DateTime.UtcNow;
                    entity.UpdatedAt = DateTime.UtcNow;


                    _context.Customers.Add(entity);
                    await _context.SaveChangesAsync();

                    RemoveByPrefix(CustomerCacheKey);

                    return entity.Id;
                }
                catch (DbUpdateException ex)
                {
                    var msg = ex.InnerException?.Message ?? ex.Message;
                    throw new Exception($"Database error while creating agency: {msg}");
                }
                catch (Exception ex)
                {
                    throw new Exception($"Unexpected error while creating agency: {ex.Message}");
                }

            }, "Customer created successfully", "Error creating customer");
        }


        public async Task<ResponseWrapper<PagedResponse<CustomerDto>>> GetAllCustomerAsync(int page = 1, int pageSize = 10)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;
            pageSize = Math.Min(pageSize, 100);

            var agencyId = _currentUser.AgencyId;

            return await ExecuteWithCacheAsync(
                cacheKey: $"{CustomerCacheKey}_{_currentUser.UserId}_{page}_{pageSize}",

                action: async () =>
                {
                    var query = _context.Customers.AsNoTracking();

                    // ✅ ALWAYS filter by agency
                    if (agencyId == Guid.Empty)
                    {
                        return new PagedResponse<CustomerDto>(
                            new List<CustomerDto>(), page, pageSize, 0
                        );
                    }

                    query = query.Where(x => x.AgencyId == agencyId);

                    var totalRecords = await query.CountAsync();

                    var mapped = await query
                        .OrderByDescending(x => x.CreatedAt)
                        .ThenBy(x => x.Id)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ProjectTo<CustomerDto>(_mapper.ConfigurationProvider)
                        .ToListAsync();

                    return new PagedResponse<CustomerDto>(mapped, page, pageSize, totalRecords);
                },

                successMessageFactory: result => $"{result.Data.Count} of {result.TotalRecords} customers fetched",
                cacheMessage: "Customers fetched from cache",
                errorMessage: "Error fetching customers"
            );
        }


        public async Task<ResponseWrapper<bool>> UpdateCustomerAsync(Guid id, UpdateCustomerDto dto)
        {
            return await ExecuteWriteAsync(async () =>
            {
                try
                {
                    var entity = await _context.Customers.FindAsync(id);

                    if (entity == null)
                        throw new Exception("Customer not found");

                    _mapper.Map(dto, entity);

                    entity.UpdatedAt = DateTime.UtcNow;
                    entity.UserId = _currentUser.UserId;
                    entity.AgencyId = _currentUser.AgencyId;
                    entity.BranchId = _currentUser.BranchId;

                    await _context.SaveChangesAsync();

                    RemoveByPrefix(CustomerCacheKey);


                    return true;
                }
                catch (DbUpdateException ex)
                {
                    var msg = ex.InnerException?.Message ?? ex.Message;
                    throw new Exception($"Database error while updating agency: {msg}");
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error updating agency: {ex.Message}");
                }

            }, "Agency updated successfully", "Error updating agency");
        }


        public async Task<ResponseWrapper<bool>> DeleteCustomerAsync(Guid id)
        {
            return await ExecuteWriteAsync(async () =>
            {
                try
                {
                    var entity = await _context.Customers.FindAsync(id);

                    if (entity == null)
                        throw new Exception("Agency not found");

                    _context.Customers.Remove(entity);
                    await _context.SaveChangesAsync();

                    RemoveByPrefix(CustomerCacheKey);


                    return true;
                }
                catch (DbUpdateException ex)
                {
                    var msg = ex.InnerException?.Message ?? ex.Message;
                    throw new Exception($"Database error while deleting agency: {msg}");
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error deleting agency: {ex.Message}");
                }

            }, "Agency deleted successfully", "Error deleting agency");
        }





    }
}
