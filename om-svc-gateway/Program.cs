using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Model;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder =>
    {
        builder.WithOrigins(["https://app.ftown.dev", "http://localhost:4200"])
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

builder.Services.AddHealthChecks();

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));


// Console.WriteLine("*******************************");
// Console.WriteLine("*******************************");
// Console.WriteLine(" ==> Config values: ");
// Console.WriteLine("Authority: " + builder.Configuration["Authentication:JwtBearer:Authority"]);
// Console.WriteLine("Audience: " + builder.Configuration["Authentication:JwtBearer:Audience"]);
// Console.WriteLine("*******************************");
// Console.WriteLine("*******************************");

builder.Services.AddAuthentication(options => 
    {
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Authentication:JwtBearer:Authority"];
        options.RequireHttpsMetadata = false;
        options.Audience = builder.Configuration["Authentication:JwtBearer:Audience"];
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            RequireSignedTokens = true,
        };
    });



/*builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("default", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("roles", "user-web", "admin");
    });
});*/

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapReverseProxy(proxy =>
{
    proxy.UseAuthorization();
    proxy.UseRouting();
});

app.MapHealthChecks("/health");

app.UseCors("CorsPolicy");

app.Run();
