using IGDB;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SavePoint.BusinessLogic.Services;
using SavePoint.BusinessLogic.Services.Interfaces;
using SavePoint.DAL.Contexts;
using SavePoint.DAL.Repositories;
using SavePoint.DAL.Repositories.Interfaces;
using System.Text.Json.Serialization;
using SavePoint.BusinessLogic.Mappings;
using SavePoint.Host.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add AutoMapper - now includes LookupMappingProfile
builder.Services.AddAutoMapper(typeof(GameMappingProfile), typeof(UserMappingProfile), typeof(LookupMappingProfile));

// Configure database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
   options.UseSqlServer(connectionString));

// Configure Identity - simplified without 2FA
builder.Services.AddIdentity<SavePoint.Entities.Users.ApplicationUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;

    // Sign in settings - No email/phone confirmation required
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure cookie authentication
builder.Services.ConfigureApplicationCookie(options =>
{
    // Cookie settings
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
    
    // API-friendly responses
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = 401;
        return Task.CompletedTask;
    };
    
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = 403;
        return Task.CompletedTask;
    };
});

// Add Authorization
builder.Services.AddAuthorization();

// Add CORS for frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "https://localhost:5173") // Vite default ports
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Important for cookie authentication
    });
});

// Register application services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IIGDBImportService, IGDBImportService>();
builder.Services.AddScoped<IGenreRepository, GenreRepository>();
builder.Services.AddScoped<IGameRepository, GameRepository>();
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<IPlatfromRepository, PlatfromRepository>();
builder.Services.AddScoped<IPopularityRepository, PopularityRepository>();
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<ILookupService, LookupService>(); // NEW: Lookup service registration

// Register DatabaseSeederService
builder.Services.AddScoped<DatabaseSeederService>();

builder.Services.AddSingleton<IGDBClient>(sp =>
{
	var clientId = "9gluavzyv9ymx4ft2u12h9dg7xah02";
	var accessToken = "zkfuz4j1qcceo99ilidwugqzzjop16";
	return new IGDBClient(clientId, accessToken);
});

var app = builder.Build();

// Seed the database with roles and default admin user
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeederService>();
    await seeder.SeedAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// CORS middleware (should be before authentication)
app.UseCors("AllowFrontend");

// Authentication & Authorization middleware (order is important!)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// No MapIdentityApi - using only custom AuthController endpoints:
// - POST /api/auth/register
// - POST /api/auth/login  
// - POST /api/auth/logout
// - GET /api/auth/me

app.Run();

// No-op email sender for development/testing
public class NoOpEmailSender : IEmailSender<SavePoint.Entities.Users.ApplicationUser>
{
    public Task SendConfirmationLinkAsync(SavePoint.Entities.Users.ApplicationUser user, string email, string confirmationLink)
    {
        // In a real application, you would send an email here
        Console.WriteLine($"Confirmation link for {email}: {confirmationLink}");
        return Task.CompletedTask;
    }

    public Task SendPasswordResetLinkAsync(SavePoint.Entities.Users.ApplicationUser user, string email, string resetLink)
    {
        // In a real application, you would send an email here
        Console.WriteLine($"Password reset link for {email}: {resetLink}");
        return Task.CompletedTask;
    }

    public Task SendPasswordResetCodeAsync(SavePoint.Entities.Users.ApplicationUser user, string email, string resetCode)
    {
        // In a real application, you would send an email here
        Console.WriteLine($"Password reset code for {email}: {resetCode}");
        return Task.CompletedTask;
    }
}
