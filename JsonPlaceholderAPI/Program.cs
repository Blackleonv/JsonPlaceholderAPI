using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using static MyService;

var builder = WebApplication.CreateBuilder(args);

// Hizmetleri kapsayýcýya ekleyin.
builder.Services.AddControllers();

// Swagger/OpenAPI'yi yapýlandýrma
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Memory Cache hizmetini ekle
builder.Services.AddMemoryCache();

// Hýz Sýnýrlamayý Yapýlandýrma
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("AspNetCoreRateLimit"));
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();


// HttpClient ve MyService'i kaydet
builder.Services.AddHttpClient();
builder.Services.AddSingleton<MyService>();

var app = builder.Build();

// HTTP istek hattýný yapýlandýrma.
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

// Hýz Sýnýrlamayý Etkinleþtir
app.UseIpRateLimiting();

app.UseAuthorization();
app.MapControllers();
app.Run();

// MyServiceRedis sýnýfýný kayýt etmek 
builder.Services.AddSingleton<MyServiceRedis>();


// Diðer servisleri ekleyin
builder.Services.AddControllers();

// Redis cache'i ekleyin
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379"; // Redis sunucusunun adresi ve portu
    options.InstanceName = "SampleInstance"; // Opsiyonel, Redis instance adý
});


// Middleware ve endpoint yapýlandýrmalarý
app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.Run();



