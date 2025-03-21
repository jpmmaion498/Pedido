using Amazon.S3;
using Amazon.SQS;
using Microsoft.EntityFrameworkCore;
using Pedido.Application.Services;
using Pedido.Domain.Interfaces;
using Pedido.Domain.Settings;
using Pedido.Infrastructure.Context;
using Pedido.Infrastructure.HostedServices;
using Pedido.Infrastructure.Repositories;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/app.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();
});

builder.Services.Configure<FeatureFlags>(
    builder.Configuration.GetSection("Features"));

builder.Services.AddScoped<IPedidoRepository, PedidoRepository>();

builder.Services.AddScoped<IPedidoService, PedidoService>();

builder.Services.AddScoped<SqsService>();

builder.Services.AddHostedService<PedidoConsumer>();

builder.Services.AddSingleton<IAmazonSQS>(sp =>
{
    var config = new AmazonSQSConfig { ServiceURL = "http://localhost:4566" };
    return new AmazonSQSClient("test", "test", config);
});

builder.Services.AddSingleton<IAmazonS3>(sp =>
{
    var config = new AmazonS3Config
    {
        ServiceURL = "http://localhost:4566",
        ForcePathStyle = true
    };
    return new AmazonS3Client("test", "test", config);
});


builder.Services.AddDbContext<PedidoDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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
