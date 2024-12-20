using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Identity;
using Scalar.AspNetCore;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Configure data protection, setup the application discriminator
// so that the data protection keys can be shared between the BFF and this API
builder.Services.AddDataProtection(o => o.ApplicationDiscriminator = "ScheduleApp");

// Configure auth
builder.Services.AddAuthentication().AddBearerToken(IdentityConstants.BearerScheme);
builder.Services.AddAuthorizationBuilder().AddCurrentUserHandler();

// Configure identity
builder.Services.AddIdentityCore<ScheduleUser>()
                .AddEntityFrameworkStores<ScheduleDbContext>()
                .AddApiEndpoints();

// Configure the database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
//builder.Services.AddSqlite<ScheduleDbContext>(connectionString);
builder.Services.AddDbContext<ScheduleDbContext>(options =>
    options.UseSqlServer(connectionString));

// State that represents the current user from the database *and* the request
builder.Services.AddCurrentUser();

// Configure Open API
builder.Services.AddOpenApi(options => options.AddBearerTokenAuthentication());

// Configure rate limiting
builder.Services.AddRateLimiting();

builder.Services.AddHttpLogging(o =>
{
    if (builder.Environment.IsDevelopment())
    {
        o.CombineLogs = true;
        o.LoggingFields = HttpLoggingFields.ResponseBody | HttpLoggingFields.ResponseHeaders;
    }
});

var app = builder.Build();

app.UseHttpLogging();
app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference(options =>
    {
        options.Servers = [];
        options.Authentication = new() { PreferredSecurityScheme = IdentityConstants.BearerScheme };
    });
}

app.MapOpenApi();

app.MapDefaultEndpoints();

app.Map("/", () => Results.Redirect("/scalar/v1"));

// Configure the APIs
app.MapSchedule();
app.MapUsers();

app.Run();
