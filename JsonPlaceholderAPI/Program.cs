using AspNetCoreRateLimit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Hosting;
using JsonPlaceholderAPI.Models;
using JsonPlaceholderAPI.Services; // MyServiceRedis s�n�f�n�n bulundu�u ad alan�n� ekleyin

var builder = WebApplication.CreateBuilder(args);

// Veritaban� ba�lant�s�n� yap�land�rma
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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

builder.Services.AddControllers();

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

app.UseHttpsRedirection();
app.UseRouting();

// H�z S�n�rlamay� Etkinle�tir
app.UseIpRateLimiting();

// Yetkilendirme middleware'ini ekle
app.UseAuthorization();

app.MapControllers();
app.Run();
