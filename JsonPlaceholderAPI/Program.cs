using AspNetCoreRateLimit;
using FluentValidation;
using FluentValidation.AspNetCore;
using JsonPlaceholderAPI.Models;
using JsonPlaceholderAPI.Repositories;  // UserRepository ve IRepository i�in
using JsonPlaceholderAPI.Services;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Controller'lar� ekleyin
builder.Services.AddControllers();

// Veritaban� ba�lant�s�n� yap�land�rma
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptions =>
        {
            sqlServerOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null);
        })
    .UseLazyLoadingProxies()  // Lazy loading'i etkinle�tir
);

// K�lt�r bilgisini yap�land�rma
var cultureInfo = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

// Swagger/OpenAPI yap�land�rmas�
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Memory Cache hizmetini ekleyin
builder.Services.AddMemoryCache();

// Redis cache'i ekleyin
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
    options.InstanceName = "SampleInstance";
});

// H�z s�n�rlama yap�land�rmas�
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("AspNetCoreRateLimit"));
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();

// HttpClient ve servisleri kaydedin
builder.Services.AddHttpClient();
builder.Services.AddSingleton<MyService>();
builder.Services.AddSingleton<MyServiceRedis>();

// Yetkilendirme hizmetlerini ekleyin
builder.Services.AddAuthorization();

// FluentValidation'� ekleyin
builder.Services.AddFluentValidationAutoValidation()
                .AddFluentValidationClientsideAdapters();

// Validat�rleri ekleyin
builder.Services.AddValidatorsFromAssemblyContaining<UserValidator>();

// UserRepository'i ekleyin
builder.Services.AddScoped<IRepository<User>, UserRepository>();

var app = builder.Build();

// HTTP istek hatt�n� yap�land�rma
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// H�z s�n�rlamay� etkinle�tirin
app.UseIpRateLimiting();

app.UseHttpsRedirection();
app.UseRouting();

// Yetkilendirme middleware'ini ekleyin
app.UseAuthorization();

app.MapControllers();
app.Run();
