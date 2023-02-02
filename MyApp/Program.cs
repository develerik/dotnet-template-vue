using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;
using MyApp;
using MyApp.Filters;
using MyApp.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// add Vue spa static files
builder.Services.AddSpaStaticFiles(o => { o.RootPath = "ClientApp/dist"; });

// configure JSON serializer with sane defaults
builder.Services.AddControllers()
  .AddJsonOptions(o => {
    o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    o.JsonSerializerOptions.WriteIndented = builder.Environment.IsDevelopment();
    o.JsonSerializerOptions.Converters.Add(
      new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
  });

// configure swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o => {
  o.EnableAnnotations();
  o.SwaggerDoc("v1", new OpenApiInfo { Title = "MyApp API", Version = "v1" });
  o.AddSecurityDefinition(
    "Bearer",
    new OpenApiSecurityScheme {
      In = ParameterLocation.Header,
      Name = "Authorization",
      Type = SecuritySchemeType.Http,
      Scheme = "bearer",
      BearerFormat = "JWT",
      Description = "Standard Bearer Authorization header. Example: `\"Bearer {token}\"`",
    });
  o.OperationFilter<AuthorizationOperationFilter>();
});

var app = builder.Build();

// configure swagger middlewares
app.UseSwagger();
app.UseSwaggerUI();

// configure authorization middlewares
app.UseAuthorization();
app.UseMiddleware<JwtMiddleware>();

// configure controller mapping
app.MapControllers();

// configure Vue spa static middleware
app.UseSpaStaticFiles();

// configure path mappings
app.MapWhen(
  x => !x.Request.Path.Value?.StartsWith("/api") ?? true,
  b => {
    b.UseSpa(spa => {
      spa.Options.SourcePath = "ClientApp";
      if (builder.Environment.IsDevelopment()) {
        spa.UseViteDevServer();
      }
    });
  }
);

app.Run();
