namespace MyApp.Middlewares;

using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;

/// <summary>
///   JWT authentication middleware.
/// </summary>
public class JwtMiddleware {
  private readonly RequestDelegate next;
  private readonly IConfiguration configuration;

  /// <summary>
  ///   Initializes a new instance of the <see cref="JwtMiddleware"/> class.
  /// </summary>
  /// <param name="next">An instance of <see cref="RequestDelegate"/>.</param>
  /// <param name="configuration">An instance of <see cref="IConfiguration"/>.</param>
  public JwtMiddleware(RequestDelegate next, IConfiguration configuration) {
    this.next = next;
    this.configuration = configuration;
  }

  /// <summary>
  ///   Invoke the middleware.
  /// </summary>
  /// <param name="context">An instance of <see cref="HttpContext"/>.</param>
  /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
  public async Task Invoke(HttpContext context) {
    var token = context.Request.Headers["Authorization"]
      .FirstOrDefault()?.Split(" ").Last();

    if (token != null) {
      await AttachUserToContext(context, token);
    }

    await next(context);
  }

  private Task AttachUserToContext(HttpContext context, string token) {
    try {
      var secret = this.configuration.GetValue<string>("Security:JwtSecret");

      var handler = new JwtSecurityTokenHandler();
      var key = Encoding.ASCII.GetBytes(secret);

      handler.ValidateToken(token,
        new TokenValidationParameters {
          ValidateIssuerSigningKey = true,
          IssuerSigningKey = new SymmetricSecurityKey(key),
          ValidateIssuer = false,
          ValidateAudience = false,
          ClockSkew = TimeSpan.Zero,
        }, out var validatedToken);

      var jwtToken = (JwtSecurityToken)validatedToken;

      // attach user to context on successful jwt validation
      if (!string.IsNullOrEmpty(jwtToken.Subject)) {
        context.Items["User"] = jwtToken.Subject;
      }
    } catch {
      // do nothing if jwt validation fails
      // user is not attached to context so request won't have access to secure routes
    }

    return Task.CompletedTask;
  }
}
