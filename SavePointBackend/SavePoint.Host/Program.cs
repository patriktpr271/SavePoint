using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SavePoint.BusinessLogic.Services;
using SavePoint.DAL.Contexts;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddIdentityCore<SavePoint.Entities.Users.ApplicationUser>();
builder.Services.AddScoped<UserService>();
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
   options.UseSqlServer(connectionString));

// Register Identity with EF Core stores
builder.Services.AddIdentity<SavePoint.Entities.Users.ApplicationUser, IdentityRole>()
	.AddEntityFrameworkStores<ApplicationDbContext>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
