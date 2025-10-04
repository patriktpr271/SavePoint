using Microsoft.AspNetCore.Identity;
using SavePoint.Host.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Register all application services using the organized configuration classes
builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

// Seed the database
await app.SeedDatabaseAsync();

// Configure the web API pipeline
app.ConfigureWebApiPipeline();

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
