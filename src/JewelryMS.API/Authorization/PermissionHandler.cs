using System.Security.Claims;
using JewelryMS.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;

public class PermissionHandler : AuthorizationHandler<PermissionRequirement> {
    private readonly IServiceScopeFactory _scopeFactory;
    public PermissionHandler(IServiceScopeFactory scopeFactory) => _scopeFactory = scopeFactory;

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement) {
        // 1. Get role from JWT Claim
        var role = context.User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        if (string.IsNullOrEmpty(role)) return;

        // 2. Resolve Repo from Scope
        using var scope = _scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IRolePermissionRepository>();

        // 3. Check DB
        if (await repo.HasPermissionAsync(role, requirement.FeatureName)) {
            context.Succeed(requirement);
        }
    }
}