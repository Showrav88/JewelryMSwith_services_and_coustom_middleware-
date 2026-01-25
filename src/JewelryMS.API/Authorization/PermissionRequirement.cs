

using Microsoft.AspNetCore.Authorization;
public class PermissionRequirement : IAuthorizationRequirement {
    public string FeatureName { get; }
    public PermissionRequirement(string featureName) => FeatureName = featureName;
}