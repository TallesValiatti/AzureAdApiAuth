using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var clientAppConfig = builder.Configuration.GetSection("SwaggerClient");
var webApiConfig = builder.Configuration.GetSection("AzureAd");

var webApiId = webApiConfig.GetSection("ClientId").Value;
var tenantId = webApiConfig.GetSection("TenantId").Value;

var authorizationUrl = clientAppConfig.GetSection("AuthorizationUrl").Value.Replace("{TenantId}", tenantId);

var tokenUrl = clientAppConfig.GetSection("TokenUrl").Value.Replace("{TenantId}", tenantId);

var clientAppId = clientAppConfig.GetSection("AppId").Value;

var clientAppSecret = clientAppConfig.GetSection("AppSecret").Value;

var scopes = clientAppConfig.GetSection("Scopes").Value.Split(" ");

// Add services to the container.
builder.Services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(x =>
{
    var scopesDict = new Dictionary<string, string>();
    foreach (var scope in scopes)
        scopesDict.Add($"api://{webApiId}/{scope}", scope);

    x.AddSecurityRequirement(new OpenApiSecurityRequirement() {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                    Id = "oauth2"
                },
                Scheme = "oauth2",
                Name = "oauth2",
                In = ParameterLocation.Header
            },
            new List <string> ()
        }
    });

    x.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            Implicit = new OpenApiOAuthFlow()
            {
                AuthorizationUrl = new Uri(authorizationUrl),
                TokenUrl = new Uri(tokenUrl),
                Scopes = scopesDict
            }
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.OAuthAppName("Swagger Client");
        options.OAuthClientId(clientAppId);
        options.OAuthClientSecret(clientAppSecret);
        options.OAuthUseBasicAuthenticationWithAccessCodeGrant();
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
