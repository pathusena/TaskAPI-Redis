using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskManagementAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<ApplicationDBContext>(options => 
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnectionString")));

builder.Services.AddStackExchangeRedisCache(options =>
{
    //options.Configuration = builder.Configuration.GetConnectionString("RedisConnectionString");
    options.ConfigurationOptions = new StackExchange.Redis.ConfigurationOptions
    {
        KeepAlive = 180,
        EndPoints = { { builder.Configuration["Redis:Host"], Convert.ToInt16(builder.Configuration["Redis:Port"]) } },
        Password = builder.Configuration["Redis:Password"],
        AbortOnConnectFail = false,
        ConnectRetry = 5,
        ConnectTimeout = 10000, // 10 seconds
    };
});


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
