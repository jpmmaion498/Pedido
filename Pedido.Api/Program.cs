using Amazon.S3;
using Amazon.SQS;
using Amazon.SQS.Model;
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
    var config = new AmazonSQSConfig { ServiceURL = "http://pedidos-localstack:4566" };
    return new AmazonSQSClient("test", "test", config);
});

builder.Services.AddSingleton<IAmazonS3>(sp =>
{
    var config = new AmazonS3Config
    {
        ServiceURL = "http://pedidos-localstack:4566",
        ForcePathStyle = true
    };
    return new AmazonS3Client("test", "test", config);
});


builder.Services.AddDbContext<PedidoDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PedidoDbContext>();

    // Retry para o banco (PostgreSQL)
    for (int i = 0; i < 5; i++)
    {
        try
        {
            dbContext.Database.Migrate();
            Console.WriteLine("? Migrations aplicadas");
            break;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"? Tentando conectar ao banco ({i + 1}/5): {ex.Message}");
            await Task.Delay(3000);
        }
    }

    var sqs = scope.ServiceProvider.GetRequiredService<IAmazonSQS>();
    var s3 = scope.ServiceProvider.GetRequiredService<IAmazonS3>();

    var queueName = "pedidos.fifo";
    var bucketName = "pedidos-arquivos";

    // Retry para LocalStack
    for (int i = 0; i < 5; i++)
    {
        try
        {
            var queues = await sqs.ListQueuesAsync(queueName);
            if (!queues.QueueUrls.Any(url => url.EndsWith(queueName)))
            {
                await sqs.CreateQueueAsync(new CreateQueueRequest
                {
                    QueueName = queueName,
                    Attributes = new Dictionary<string, string>
                    {
                        { "FifoQueue", "true" },
                        { "ContentBasedDeduplication", "true" }
                    }
                });
                Console.WriteLine("? Fila criada");
            }

            var buckets = await s3.ListBucketsAsync();
            if (!buckets.Buckets.Any(b => b.BucketName == bucketName))
            {
                await s3.PutBucketAsync(bucketName);
                Console.WriteLine("? Bucket criado");
            }

            break; // sucesso
        }
        catch (Exception ex)
        {
            Console.WriteLine($"? Tentando conectar ao LocalStack ({i + 1}/5): {ex.Message}");
            await Task.Delay(3000);
        }
    }
}





app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
