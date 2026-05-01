using AutenticationWeb.API;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.



builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddRateLimiter( rateLimiterOption =>
{
    rateLimiterOption.AddFixedWindowLimiter("fixed", option =>
    {
        option.PermitLimit = 5; // Allow 5 requests
        option.Window = TimeSpan.FromSeconds(10); // Per 10 seconds
        option.QueueProcessingOrder = QueueProcessingOrder.OldestFirst; // Process queued requests in order
        option.QueueLimit = 2; // Allow up to 2 queued requests
    });

    rateLimiterOption.AddSlidingWindowLimiter("sliding", option =>
    {
        option.PermitLimit = 5;
        option.Window = TimeSpan.FromSeconds(10);
        option.SegmentsPerWindow = 2; // Divide the window into 2 segments (5 seconds each)
        option.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        option.QueueLimit = 2;
    });

    rateLimiterOption.AddTokenBucketLimiter("token", option =>
    {
        option.TokenLimit = 10; // Maximum 10 tokens
        option.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        option.QueueLimit = 2;
        option.ReplenishmentPeriod = TimeSpan.FromSeconds(10); // Replenish tokens every 10 seconds
        option.TokensPerPeriod = 5; // Add 5 tokens per replenishment period
    });
});

builder.Services.AddHttpClient("ResilientClient")
    .AddResilienceHandler("my-pipeline", pipeline =>
    {
        pipeline.AddRetry(new HttpRetryStrategyOptions 
        { 
            MaxRetryAttempts = 5,
            Delay = TimeSpan.FromSeconds(2),
        });

        pipeline.AddTimeout(TimeSpan.FromSeconds(2));

        pipeline.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
        {
            FailureRatio = 0.5, // 50% failure rate to open the circuit
            MinimumThroughput = 10, // Minimum 10 requests before evaluating failure rate
            SamplingDuration = TimeSpan.FromSeconds(10), // Evaluate failures over a 10-second window 
            BreakDuration = TimeSpan.FromSeconds(30) // Break for 30 seconds
        });
    });

builder.Services.AddDbContext<ApplicationDbContext>
    (option => option.UseSqlServer("Data Source=SATTYAMMISHRA;Database=SattyamDB;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Application Name=\"SQL Server Management Studio\";Command Timeout=0"));


builder.Services.AddIdentity<IdentityUser, IdentityRole>(
    option => { 
        option.Password.RequireDigit = true;
        option.Password.RequiredLength = 8;
        option.Password.RequireNonAlphanumeric = true;
        option.Password.RequireUppercase = true;
        option.Password.RequireLowercase = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddControllers();

var app = builder.Build();


if(app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        options.RoutePrefix = string.Empty; // Swagger UI at root (http://localhost:<port>/)
    });
}

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.UseRateLimiter();

app.MapControllers()
    .RequireRateLimiting("fixed");



app.Run();
