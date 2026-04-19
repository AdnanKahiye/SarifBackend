namespace Backend.Interfaces
{
    public interface ICurrentUserService
    {
        string? UserId { get; }
        string? Email { get; }
        bool IsAuthenticated { get; }

        Guid AgencyId { get; }
        Guid ? BranchId { get; }

        bool IsInRole(string role); 
    }
}
