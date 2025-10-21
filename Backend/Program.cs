using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Azure.Cosmos;
using Microsoft.IdentityModel.Tokens;
using ColumbiaAi.Backend.Configuration;
using ColumbiaAi.Backend.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuration
var azureOpenAIConfig = builder.Configuration.GetSection("AzureOpenAI").Get<AzureOpenAIConfig>() ?? new();
var cosmosDbConfig = builder.Configuration.GetSection("CosmosDb").Get<CosmosDbConfig>() ?? new();
var blobStorageConfig = builder.Configuration.GetSection("BlobStorage").Get<BlobStorageConfig>() ?? new();
var cognitiveSearchConfig = builder.Configuration.GetSection("CognitiveSearch").Get<CognitiveSearchConfig>() ?? new();
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>() ?? new();
var featureFlags = builder.Configuration.GetSection("Features").Get<FeatureFlags>() ?? new();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Application Insights
builder.Services.AddApplicationInsightsTelemetry();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Azure Services
builder.Services.AddSingleton(azureOpenAIConfig);
builder.Services.AddSingleton(cosmosDbConfig);
builder.Services.AddSingleton(blobStorageConfig);
builder.Services.AddSingleton(cognitiveSearchConfig);
builder.Services.AddSingleton(jwtSettings);
builder.Services.AddSingleton(featureFlags);

// Cosmos DB
builder.Services.AddSingleton<CosmosClient>(sp =>
{
    return new CosmosClient(cosmosDbConfig.Endpoint, cosmosDbConfig.Key);
});

// Register Services
builder.Services.AddSingleton<ICosmosDbService, CosmosDbService>();
builder.Services.AddSingleton<IAzureOpenAIService, AzureOpenAIService>();
builder.Services.AddSingleton<IBlobStorageService, BlobStorageService>();
builder.Services.AddSingleton<ICognitiveSearchService, CognitiveSearchService>();
builder.Services.AddSingleton<IJwtService, JwtService>();
builder.Services.AddSingleton<IPasswordService, PasswordService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
