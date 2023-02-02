using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;
using MyApp;
using MyApp.Filters;

var builder = WebApplication.CreateBuilder(args);

// add Vue spa static files
builder.Services.AddSpaStaticFiles(options => { options.RootPath = "ClientApp/dist"; });

// configure JSON serializer with sane defaults
builder.Services.AddControllers()
  .AddJsonOptions(options => {
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.WriteIndented = builder.Environment.IsDevelopment();
    options.JsonSerializerOptions.Converters.Add(
      new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
  });

// configure swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
  c.EnableAnnotations();
  c.SwaggerDoc("v1", new OpenApiInfo { Title = "MyApp API", Version = "v1" });
  c.AddSecurityDefinition(
    "Bearer",
    new OpenApiSecurityScheme {
      In = ParameterLocation.Header,
      Name = "Authorization",
      Type = SecuritySchemeType.Http,
      Scheme = "bearer",
      BearerFormat = "JWT",
      Description = "Standard Bearer Authorization header. Example: `\"Bearer {token}\"`",
    });
  c.OperationFilter<AuthorizationOperationFilter>();
});

var app = builder.Build();

// configure swagger middlewares
app.UseSwagger();
app.UseSwaggerUI();

// configure authorization middleware
app.UseAuthorization();

// configure controller mapping
app.MapControllers();

// configure Vue spa static middleware
app.UseSpaStaticFiles();

// configure path mappings
app.MapWhen(
  x => !x.Request.Path.Value?.StartsWith("/api") ?? true,
  c => {
    c.UseSpa(spa => {
      spa.Options.SourcePath = "ClientApp";
      if (builder.Environment.IsDevelopment()) {
        spa.UseViteDevServer();
      }
    });
  }
);

app.Run();
