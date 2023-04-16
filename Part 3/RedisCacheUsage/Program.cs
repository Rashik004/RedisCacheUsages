using RedisUsage.ConcurrencyGuard;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

RegisterRedisCache(builder);
AddRedisCacheForDistributedCaching(builder);
builder.Services.AddSingleton<ConcurrencyGuard>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();

void AddRedisCacheForDistributedCaching(WebApplicationBuilder builder)
{
    var redisConnectionString = builder.Configuration.GetSection("RedisConnectionString").Value;
    var configurationOptions = ConfigurationOptions.Parse(redisConnectionString);
    configurationOptions.ClientName="RedisUsage.IDistributedCacheClient";
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.ConfigurationOptions=configurationOptions;
    });
}

void RegisterRedisCache(WebApplicationBuilder builder)
{
    var redisConnectionString = builder.Configuration.GetSection("RedisConnectionString").Value;
    var configurationOptions = ConfigurationOptions.Parse(redisConnectionString);
    configurationOptions.ClientName="RedisUsage.GeneralClient";
    var multiplexer = ConnectionMultiplexer.Connect(configurationOptions);
    builder.Services.AddSingleton<IConnectionMultiplexer>(multiplexer);
}

public partial class Program {}
