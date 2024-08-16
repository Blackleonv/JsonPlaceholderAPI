using AspNetCoreRateLimit;
using FluentValidation;
using FluentValidation.AspNetCore;
using JsonPlaceholderAPI.Models;
using JsonPlaceholderAPI.Services;
using Microsoft.EntityFrameworkCore;
using System.Globalization;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

// Veritabaný baðlantýsýný yapýlandýrma
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



// Swagger/OpenAPI'yi yapýlandýrma
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

// Hýz Sýnýrlamayý Yapýlandýrma
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

// FluentValidation'ý ekleyin
builder.Services.AddFluentValidationAutoValidation()
                .AddFluentValidationClientsideAdapters();

// Validatörleri ekleyin
builder.Services.AddValidatorsFromAssemblyContaining<UserValidator>();

var app = builder.Build();

// HTTP istek hattýný yapýlandýrma
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


// Hýz Sýnýrlamayý Etkinleþtir
app.UseIpRateLimiting();

app.UseHttpsRedirection();
app.UseRouting();

// Yetkilendirme middleware'ini ekle
app.UseAuthorization();

app.MapControllers();
app.Run();