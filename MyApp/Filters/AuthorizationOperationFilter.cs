namespace MyApp.Filters;

using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using AuthorizeAttribute = MyApp.Attributes.AuthorizeAttribute;

/// <summary>
///   Swagger authorization filter.
/// </summary>
public class AuthorizationOperationFilter : IOperationFilter {
  /// <inheritdoc />
  public void Apply(OpenApiOperation operation, OperationFilterContext context) {
    var actionMetadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;
    var isAuthorized = actionMetadata.Any(o => o is AuthorizeAttribute);
    var allowAnonymous = actionMetadata.Any(o => o is AllowAnonymousAttribute);

    if (!isAuthorized || allowAnonymous) {
      return;
    }

    operation.Parameters ??= new List<OpenApiParameter>();
    operation.Security = new List<OpenApiSecurityRequirement> {
      new OpenApiSecurityRequirement {
        {
          new OpenApiSecurityScheme {
            Reference = new OpenApiReference {
              Id = "Bearer",
              Type = ReferenceType.SecurityScheme,
            },
          },
          Array.Empty<string>()
        },
      },
    };
  }
}
