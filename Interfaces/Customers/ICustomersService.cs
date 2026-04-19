using Backend.DTOs.Requests.Customers;
using Backend.DTOs.Requests.Setup;
using Backend.DTOs.Responses.Customers;
using Backend.Wrapper;

namespace Backend.Interfaces.Customers
{
    public interface ICustomersService
    {
        Task<ResponseWrapper<Guid>> CreateCustomerAsync(CreateCustomerDto dto);

        Task<ResponseWrapper<PagedResponse<CustomerDto>>> GetAllCustomerAsync(int page = 1, int pageSize = 10);

        Task<ResponseWrapper<bool>> UpdateCustomerAsync(Guid id, UpdateCustomerDto dto);

        Task<ResponseWrapper<bool>> DeleteCustomerAsync(Guid id);
    }
}
