using System.Net;
using System.Reflection;
using System.Text.Json.Serialization;
using dotenv.net;
using JewelryStoreBackend;
using JewelryStoreBackend.Filters;
using JewelryStoreBackend.Repository;
using JewelryStoreBackend.Repository.Interfaces;
using JewelryStoreBackend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using Serilog;
using Serilog.Events;
using StackExchange.Redis;
using ServerVersion = Microsoft.EntityFrameworkCore.ServerVersion;

DotEnv.Load();

var builder = WebApplication.CreateBuilder(args);


// Регистрация фильтров
builder.Services.AddScoped<ValidateUserIpFilter>();
builder.Services.AddScoped<ValidateJwtAccessTokenFilter>();

// Регистрация Сервисов
builder.Services.AddScoped<AuthorizationService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<BasketService>();

// Регистрация репозиториев
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IBasketRepository, BasketRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IAddressRepository, AddressRepository>();
builder.Services.AddScoped<ICouponRepository, CouponRepository>();

builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<WarehouseRepository>();

// Логгирование
builder.Services.AddLogging(config =>
{
    config.AddDebug();
    config.AddConsole();
    config.SetMinimumLevel(LogLevel.Error);
});

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Hour,
        restrictedToMinimumLevel: LogEventLevel.Information)
    .WriteTo.Console()
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.SetMinimumLevel(LogLevel.Error);
builder.Logging.AddSerilog();

// Swagger
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});


// Подключение Jwt токена
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => {
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = AuthOptions.Issuer,
        ValidateAudience = true,
        ValidAudience = AuthOptions.Audience,
        ValidateLifetime = true,
        IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
        ValidateIssuerSigningKey = true
    };
    
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                context.Response.Headers.Append("Token-Expired", "true");
            return Task.CompletedTask;
        }
    };
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownProxies.Add(IPAddress.Parse("::ffff:172.17.0.1")); // IP прокси/контейнера
});


// определяем MongoClient как синглтон
var mongoDbConnectionString = builder.Configuration.GetConnectionString("MongoDBConnection");
builder.Services.AddSingleton(new MongoClient(mongoDbConnectionString));
builder.Services.AddSingleton<IMongoClient>(new MongoClient(mongoDbConnectionString));


// Подключаем Redis
var redisConnectionString = builder.Configuration.GetConnectionString("RedisConnection") ?? throw new InvalidOperationException("Redis connection string is not configured.");
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));


// Подключает БД
string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationContext>(options => options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));


// Регистрация S3Service в DI контейнере
builder.Services.AddSingleton<S3Service>();


// Добавляем контроллеры
builder.Services.AddControllers(options =>
{
    options.Filters.Add(new AuthorizeFilter());
});

builder.Services.AddControllers()
    .AddJsonOptions(options => { options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); });
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options => {
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Введите JWT токен авторизации.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    
    options.OperationFilter<AuthorizeCheckOperationFilter>();

    var xmlFileName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFileName));
});
builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();


// Добавляем CORS
builder.Services.AddCors();


var app = builder.Build();


// Настройка среды
app.UseDeveloperExceptionPage();
app.UseStatusCodePages();
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
        
    foreach (var description in provider.ApiVersionDescriptions)
    {
        options.SwaggerEndpoint(
            $"/swagger/{description.GroupName}/swagger.json",
            description.GroupName.ToUpperInvariant());
    }
    options.RoutePrefix = string.Empty;
});

app.UseCors(policyBuilder => policyBuilder
    .WithOrigins(
        "http://localhost:5173", 
        "http://localhost:5174", 
        "http://localhost:5175"
    )
    .AllowAnyHeader()
    .WithMethods("GET", "POST", "PUT", "DELETE")
    .AllowCredentials()
);

app.UseForwardedHeaders();

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();