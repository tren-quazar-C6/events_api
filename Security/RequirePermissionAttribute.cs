using Microsoft.AspNetCore.Authorization;

namespace events_api.Security;

public sealed class RequirePermissionAttribute : AuthorizeAttribute
{
    public RequirePermissionAttribute(string permission)
    {
        Policy = $"perm:{permission}";
    }
}
