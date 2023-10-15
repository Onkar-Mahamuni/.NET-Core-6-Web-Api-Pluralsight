using CityInfo.API.DbContexts;
using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Reflection;
using System.Text;

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
builder.Services.AddEndpointsApiExplorer(); //Used by Swashbuckle to generate api specifications
builder.Services.AddSwaggerGen(setupAction =>
{ // These are the parameters for swagger api documentation configuration
    var xmlCommentsFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"; // api docs file path
    var xmlCommentsFullPath = Path.Combine(AppContext.BaseDirectory, xmlCommentsFile); // Full path of the file

    setupAction.IncludeXmlComments(xmlCommentsFullPath); // To tell Swashbuckle to read the docs from

    setupAction.AddSecurityDefinition("CityInfoApiBearerAuth", new OpenApiSecurityScheme() // This is to add authentication support to our documentation 
    {
        Type = SecuritySchemeType.Http, // Specify security type
        Scheme = "Bearer", // Scurity scheme like OAuth, OpenAPI or Bearer
        Description = "Input a valid token to access this API" // The message we want
    });

    setupAction.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "CityInfoApiBearerAuth"
                }
            }, new List<string>() //Used when tokens in scope, but we are not using scopes hence passing an aempty list
        } 
    });

}); //Registers the services that are used to effectively generate the specs

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

// To add automapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

//For token validation
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Authentication:Issuer"],
            ValidAudience = builder.Configuration["Authentication:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["Authentication:SecretForKey"]))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("MustBeFromABC", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("city", "ABC");
    });
});

builder.Services.AddApiVersioning(setupAction =>
{
    setupAction.AssumeDefaultVersionWhenUnspecified = true;
    setupAction.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
    setupAction.ReportApiVersions = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//Authentication should be before authorization
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();