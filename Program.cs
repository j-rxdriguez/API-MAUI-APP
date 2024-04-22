using System.Text;
using BlazorMauiAppAPI.Data;
using BlazorMauiAppAPI.Data.Models;
using BlazorMauiAppAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Adding services to the container.
builder.Services.AddDbContext<BlazorMauiAppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MauiProjectDbConn")));

// Retrieving the TokenSecret from configuration
var tokenSecret = builder.Configuration.GetSection("AppSettings").GetValue<string>("TokenSecret");

if (string.IsNullOrWhiteSpace(tokenSecret))
{
    throw new InvalidOperationException("The TokenSecret configuration must not be null or empty.");
}

var tokenValidationParameters = new TokenValidationParameters
{
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(tokenSecret)),
    ValidateIssuer = false,
    ValidateAudience = false
};


// authentication services
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(jwt =>
{
    jwt.TokenValidationParameters = tokenValidationParameters;
});

IdentityBuilder bdl = builder.Services.AddIdentityCore<User>(opt =>
{
    opt.Password.RequireDigit = false;
    opt.Password.RequiredLength = 4;
    opt.Password.RequireNonAlphanumeric = false;
    opt.Password.RequireUppercase = false;
})
                .AddDefaultTokenProviders();
bdl = new IdentityBuilder(bdl.UserType, typeof(Role), builder.Services);
bdl.AddEntityFrameworkStores<BlazorMauiAppDbContext>();
bdl.AddRoleValidator<RoleValidator<Role>>();
bdl.AddRoleManager<RoleManager<Role>>();
bdl.AddSignInManager<SignInManager<User>>();
bdl.AddUserManager<UserManager<User>>();

builder.Services.AddScoped<IGenerateToken, GenerateTokenAsync>();
builder.Services.AddScoped<IDataService, DataService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("JesusPolicy", options => {
        options
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("JesusPolicy");

app.UseHttpsRedirection();

app.UseAuthentication();



app.UseAuthorization();

app.MapControllers();

app.Run();
