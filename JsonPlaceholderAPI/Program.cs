using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Hizmetleri kapsay�c�ya ekleyin.
builder.Services.AddControllers();

// Swagger/OpenAPI'yi yap�land�rma
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Memory Cache hizmetini ekle
builder.Services.AddMemoryCache();

// H�z S�n�rlamay� Yap�land�rma
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("AspNetCoreRateLimit"));
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();

// HttpClient ve MyService'i kaydet
builder.Services.AddHttpClient();
builder.Services.AddSingleton<MyService>();

var app = builder.Build();

// HTTP istek hatt�n� yap�land�rma.
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

app.UseAuthorization();
app.MapControllers();
app.Run();



