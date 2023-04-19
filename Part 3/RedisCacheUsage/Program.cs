using RedisUsage.ConcurrencyGuard;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var multiplexer = RegisterRedisCache(builder);
AddRedisCacheForDistributedCaching(builder, multiplexer);
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

void AddRedisCacheForDistributedCaching(WebApplicationBuilder builder, ConnectionMultiplexer multiplexer)
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.ConnectionMultiplexerFactory = async () => { return await Task.FromResult(multiplexer); };
    });
}

ConnectionMultiplexer RegisterRedisCache(WebApplicationBuilder builder)
{
    var redisConnectionString = builder.Configuration.GetSection("RedisConnectionString").Value;
    var configurationOptions = ConfigurationOptions.Parse(redisConnectionString);
    configurationOptions.ClientName="RedisUsage.GeneralClient";
    var multiplexer = ConnectionMultiplexer.Connect(configurationOptions);
    builder.Services.AddSingleton<IConnectionMultiplexer>(multiplexer);
    return multiplexer;
}

public partial class Program {}
