using AspNetCoreRateLimit;
using FluentValidation;
using FluentValidation.AspNetCore;
using JsonPlaceholderAPI.Models;
using JsonPlaceholderAPI.Repositories;  // UserRepository ve IRepository için
using JsonPlaceholderAPI.Services;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Controller'larý ekleyin
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
        })
    .UseLazyLoadingProxies()  // Lazy loading'i etkinleþtir
);

// Kültür bilgisini yapýlandýrma
var cultureInfo = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

// Swagger/OpenAPI yapýlandýrmasý
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

// Hýz sýnýrlama yapýlandýrmasý
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

// FluentValidation'ý ekleyin
builder.Services.AddFluentValidationAutoValidation()
                .AddFluentValidationClientsideAdapters();

// Validatörleri ekleyin
builder.Services.AddValidatorsFromAssemblyContaining<UserValidator>();

// UserRepository'i ekleyin
builder.Services.AddScoped<IRepository<User>, UserRepository>();

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

// Hýz sýnýrlamayý etkinleþtirin
app.UseIpRateLimiting();

app.UseHttpsRedirection();
app.UseRouting();

// Yetkilendirme middleware'ini ekleyin
app.UseAuthorization();

app.MapControllers();
app.Run();
