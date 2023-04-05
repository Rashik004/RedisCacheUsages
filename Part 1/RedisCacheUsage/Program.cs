using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
RegisterRedisCache(builder);


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


void RegisterRedisCache(WebApplicationBuilder builder)
{
    var redisConnectionString = builder.Configuration.GetSection("RedisConnectionString");
    var multiplexer = ConnectionMultiplexer.Connect(redisConnectionString.Value);
    builder.Services.AddSingleton<IConnectionMultiplexer>(multiplexer);
}