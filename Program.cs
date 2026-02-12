using Base.Api.Data;
using Base.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Connection
builder.Services.AddSingleton<Connection>();

// Add controllers
builder.Services.AddControllers();

// CORS
//builder.Services.AddCustomCors(new[] { "https://frontend.example.com", "http://localhost:3000" });

// Add application services
builder.Services.AddAppServices();

// Add Swagger / OpenAPI
builder.Services.AddSwaggerDocumentation();

// Add JWT authentication
builder.Services.AddJwtAuthentication(builder.Configuration);
// Rate Limit
builder.Services.AddRateLimiting(builder.Configuration);


var app = builder.Build();

// Enable Swagger middleware only in Development
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerDocumentation();
}

app.UseHttpsRedirection();

// Authentication & Authorization
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiting();
//app.UseCustomCors();

// Map controllers
app.MapControllers();

app.Run();
