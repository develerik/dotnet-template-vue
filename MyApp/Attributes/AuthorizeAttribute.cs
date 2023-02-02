namespace MyApp.Attributes;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

/// <summary>
///   Authorization attribute for route protection.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeAttribute : Attribute, IAuthorizationFilter {
  /// <inheritdoc />
  public void OnAuthorization(AuthorizationFilterContext context) {
    var user = context.HttpContext.Items["User"];

    if (user == null) {
      context.Result = new StatusCodeResult(StatusCodes.Status401Unauthorized);
    }
  }
}
