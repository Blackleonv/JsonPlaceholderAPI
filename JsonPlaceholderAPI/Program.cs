using AspNetCoreRateLimit;
using FluentValidation;
using FluentValidation.AspNetCore;
using JsonPlaceholderAPI.Models;
using JsonPlaceholderAPI.Services;
using Microsoft.EntityFrameworkCore;
using System.Globalization;


var builder = WebApplication.CreateBuilder(args);
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
        }));


var cultureInfo = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;



// Swagger/OpenAPI'yi yap�land�rma
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Memory Cache hizmetini ekle
builder.Services.AddMemoryCache();

// Redis cache'i ekleyin
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
    options.InstanceName = "SampleInstance";
});

// H�z S�n�rlamay� Yap�land�rma
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("AspNetCoreRateLimit"));
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();

// HttpClient ve servisleri kaydet
builder.Services.AddHttpClient();
builder.Services.AddSingleton<MyService>();
builder.Services.AddSingleton<MyServiceRedis>();

// Yetkilendirme hizmetlerini ekle
builder.Services.AddAuthorization();

// FluentValidation'� ekleyin
builder.Services.AddFluentValidationAutoValidation()
                .AddFluentValidationClientsideAdapters();

// Validat�rleri ekleyin
builder.Services.AddValidatorsFromAssemblyContaining<UserValidator>();

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


// H�z S�n�rlamay� Etkinle�tir
app.UseIpRateLimiting();

app.UseHttpsRedirection();
app.UseRouting();

// Yetkilendirme middleware'ini ekle
app.UseAuthorization();

app.MapControllers();
app.Run();