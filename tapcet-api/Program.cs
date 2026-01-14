using Microsoft.EntityFrameworkCore;
using tapcet_api.Data;
using tapcet_api.Extensions; 
using tapcet_api.Services.Implementations;
using tapcet_api.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Infrastructure Services ---
builder.Services.AddControllers();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Extension Modules ---
builder.Services.AddIdentityServices(builder.Configuration); 
builder.Services.AddSwaggerDocumentation();                  
builder.Services.AddAutoMapper(typeof(Program).Assembly);

//  Application Services (Business Logic) ---
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IQuizService, QuizService>();
builder.Services.AddScoped<IQuizAttemptService, QuizAttemptService>();

var app = builder.Build();


await DbSeeder.SeedAsync(app.Services);


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();