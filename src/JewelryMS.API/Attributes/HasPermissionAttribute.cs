
using Microsoft.AspNetCore.Authorization;
public class HasPermissionAttribute : AuthorizeAttribute {
    public HasPermissionAttribute(string featureName) : base(featureName) { }
}