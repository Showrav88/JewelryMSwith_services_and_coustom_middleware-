using System.Security.Claims;
using JewelryMS.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

public class PermissionHandler : AuthorizationHandler<PermissionRequirement> 
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PermissionHandler(IServiceScopeFactory scopeFactory, IHttpContextAccessor httpContextAccessor)
    {
        _scopeFactory = scopeFactory;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var role = context.User.FindFirst(ClaimTypes.Role)?.Value;
        if (string.IsNullOrEmpty(role)) return;

        using var scope = _scopeFactory.CreateScope();
        // Resolve Service instead of Repo
        var permissionService = scope.ServiceProvider.GetRequiredService<IPermissionService>();

        if (await permissionService.HasPermissionAsync(role, requirement.FeatureName)) 
        {
            context.Succeed(requirement);
        }
        else 
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                // Custom message triggered when DB returns 'false'
                httpContext.Items["ForbiddenMessage"] = $"Access Denied: Role '{role}' lacks permission for '{requirement.FeatureName}'.";
            }
        }
        
    }
}