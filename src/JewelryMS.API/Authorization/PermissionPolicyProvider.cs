using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace JewelryMS.API.Authorization;

public class PermissionPolicyProvider : DefaultAuthorizationPolicyProvider
{
    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options) : base(options) { }

    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        // Check if the policy exists (like "Default" or "AdminOnly")
        var policy = await base.GetPolicyAsync(policyName);
        if (policy != null) return policy;

        // If not, create a dynamic policy using the feature name as the requirement
        return new AuthorizationPolicyBuilder()
            .AddRequirements(new PermissionRequirement(policyName))
            .Build();
    }
}