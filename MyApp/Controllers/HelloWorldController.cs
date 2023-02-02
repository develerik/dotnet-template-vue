namespace MyApp.Controllers;

using Microsoft.AspNetCore.Mvc;
using MyApp.Attributes;
using Swashbuckle.AspNetCore.Annotations;

/// <summary>
///   Hello World example controller.
/// </summary>
[ApiController]
[Route("api")]
[SwaggerTag("Hello World examples")]
public class HelloWorldController : Controller {
  [Route("hello")]
  [HttpGet]
  [SwaggerOperation(
    Summary = "Unsecure Endpoint",
    OperationId = "Hello",
    Tags = new[] { "Example" })]
  public string Hello() {
    return "Hello World!";
  }

  [Authorize]
  [Route("hello-secure")]
  [HttpGet]
  [SwaggerOperation(
    Summary = "Secure Endpoint",
    OperationId = "HelloSecure",
    Tags = new[] { "Example" })]
  public string HelloSecure() {
    return $"Secure Hello World! JWT Sub: {HttpContext.Items["User"]}";
  }
}
