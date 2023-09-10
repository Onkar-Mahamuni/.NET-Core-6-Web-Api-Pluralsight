using CityInfo.API.DbContexts;
using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Serilog;

//Logger configuration for serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/cityinfo.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Clearing default log provider and adding a custom log provider as a console
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// To let the container know that we need to use serilog now
builder.Host.UseSerilog();

// Add services to the container.

builder.Services.AddControllers(
    options =>
    {
        //options.OutputFormatters - can be used if we need to set any format at default, by default its JSON
        options.ReturnHttpNotAcceptable = true;
    }
).AddNewtonsoftJson() // Set default input o/p formaters to newtonsoftjson
 .AddXmlDataContractSerializerFormatters(); // adds support for xml response from all APIs

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Injects a content type of the file 
builder.Services.AddSingleton<FileExtensionContentTypeProvider>();

// To register mail service so we can inject it using built in dependency injection

#if DEBUG //Using compiler symbols
builder.Services.AddTransient<IMailService, LocalMailService>();
#else
builder.Services.AddTransient<IMailService, CloudMailService>();
#endif

// Transient - created at each request of the resource, best for lightweight & stateless systems
// Scoped - once per request 
// Singleton - created at first request only, all subsequent requests will be shared the same object

builder.Services.AddSingleton<CitiesDataStore>();


//Way 2. To initialize the database- To let it know where to find this database
builder.Services.AddDbContext<CityInfoContext>(
    DbContextOptions => DbContextOptions.UseSqlite(builder.Configuration["ConnectionStrings:CityInfoDBConnectionString"]));

//Environment variable overridides all other variables in connection environment

builder.Services.AddScoped<ICityInfoRepository, CityInfoRepository>();


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