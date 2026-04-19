using Hangfire.Dashboard;

namespace Backend.Utiliy
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            // Example 1: allow all (for testing only)
            return true;

            // 🔒 Later you will secure it properly
        }
    }
}
