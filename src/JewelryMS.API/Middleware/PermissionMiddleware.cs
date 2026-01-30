namespace JewelryMS.API.Middleware;
public class PermissionMiddleware
{
    private readonly RequestDelegate _next;

    public PermissionMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);

        // If authorization failed
        if (context.Response.StatusCode == StatusCodes.Status403Forbidden && !context.Response.HasStarted)
        {
            var message = context.Items["ForbiddenMessage"]?.ToString() ?? "Forbidden: Access Denied.";

            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new 
            { 
                error = "AccessDenied",
                message = message,
                status = 403
            });
            
        }
    }
}