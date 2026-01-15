using Microsoft.EntityFrameworkCore;
using tapcet_api.Data;
using tapcet_api.Extensions; // Important!
using tapcet_api.Services.Implementations;
using tapcet_api.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// --- 1. Infrastructure Services ---
builder.Services.AddControllers();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- 2. Custom Extension Modules ---
builder.Services.AddIdentityServices(builder.Configuration); // Clean!
builder.Services.AddSwaggerDocumentation();                  // Clean!
builder.Services.AddRateLimitingServices(builder.Configuration);
builder.Services.AddAutoMapper(typeof(Program).Assembly);

// --- 3. Application Services (Business Logic) ---
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IQuizService, QuizService>();
builder.Services.AddScoped<IQuizAttemptService, QuizAttemptService>();

var app = builder.Build();

// --- 4. Database Initialization ---
await DbSeeder.SeedAsync(app.Services);

// --- 5. Request Pipeline (Middleware) ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();

app.MapControllers();
app.Run();