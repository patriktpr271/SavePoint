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
builder.Services.AddIdentityCore<SavePoint.Entities.Users.ApplicationUser>();

// Add AutoMapper - now includes LookupMappingProfile
builder.Services.AddAutoMapper(typeof(GameMappingProfile), typeof(UserMappingProfile), typeof(LookupMappingProfile));

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IIGDBImportService, IGDBImportService>();
builder.Services.AddScoped<IGenreRepository, GenreRepository>();
builder.Services.AddScoped<IGameRepository, GameRepository>();
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<IPlatfromRepository, PlatfromRepository>();
builder.Services.AddScoped<IPopularityRepository, PopularityRepository>();
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<ILookupService, LookupService>(); // NEW: Lookup service registration

builder.Services.AddSingleton<IGDBClient>(sp =>
{
	var clientId = "9gluavzyv9ymx4ft2u12h9dg7xah02";
	var accessToken = "zkfuz4j1qcceo99ilidwugqzzjop16";
	return new IGDBClient(clientId, accessToken);
});

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
