using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Server.Service;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddScoped<UserService>();
builder.Services.AddHttpClient(); 
builder.Services.AddScoped<ApiService>();
builder.Services.AddHttpClient<TokenService>();




builder.Services.AddControllers();

builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("SCIMPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://your-client-domain.com") 
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Enable Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Secret Server API v1");
});

// Use CORS policy
app.UseCors("SCIMPolicy");

app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();
