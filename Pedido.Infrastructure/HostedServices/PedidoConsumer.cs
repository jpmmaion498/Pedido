using Amazon.SQS;
using Amazon.SQS.Model;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Pedido.Domain.Entities;
using Pedido.Domain.Interfaces;
using System.Text.Json;
using System.Text.Json.Serialization;
using Serilog;

namespace Pedido.Infrastructure.HostedServices;

public class PedidoConsumer : BackgroundService
{
    private readonly IAmazonSQS _sqsClient;
    private readonly IAmazonS3 _s3Client;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly string _queueUrl = "http://localhost:4566/000000000000/pedidos.fifo";

    public PedidoConsumer(IAmazonSQS sqsClient, IAmazonS3 s3Client, IServiceScopeFactory scopeFactory)
    {
        _sqsClient = sqsClient;
        _s3Client = s3Client;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Log.Information("PedidoConsumer iniciado");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var response = await _sqsClient.ReceiveMessageAsync(new ReceiveMessageRequest
                {
                    QueueUrl = _queueUrl,
                    MaxNumberOfMessages = 5,
                    WaitTimeSeconds = 10
                }, stoppingToken);

                foreach (var message in response.Messages)
                {
                    try
                    {
                        PedidoEntity? pedido;

                        if (message.Body.Contains("\"storageType\":\"s3\""))
                        {
                            var meta = JsonSerializer.Deserialize<S3Reference>(message.Body);
                            Log.Information("Baixando pedido do S3: {Bucket}/{Key}", meta?.Bucket, meta?.Key);

                            var s3Object = await _s3Client.GetObjectAsync(meta.Bucket, meta.Key);
                            using var reader = new StreamReader(s3Object.ResponseStream);
                            var pedidoJson = await reader.ReadToEndAsync();

                            pedido = JsonSerializer.Deserialize<PedidoEntity>(pedidoJson);
                        }
                        else
                        {
                            pedido = JsonSerializer.Deserialize<PedidoEntity>(message.Body);
                        }

                        if (pedido is not null)
                        {
                            using var scope = _scopeFactory.CreateScope();
                            var repo = scope.ServiceProvider.GetRequiredService<IPedidoRepository>();
                            await repo.AdicionarAsync(pedido);

                            Log.Information("Pedido {PedidoId} salvo com sucesso", pedido.PedidoId);
                        }

                        await _sqsClient.DeleteMessageAsync(_queueUrl, message.ReceiptHandle, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Erro ao processar mensagem da fila: {Message}", ex.Message);
                    }
                }

                await Task.Delay(1000, stoppingToken);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Erro no ReceiveMessageAsync: {Message}", ex.Message);
            }
        }

        Log.Information("PedidoConsumer finalizado");
    }

    private class S3Reference
    {
        [JsonPropertyName("storageType")]
        public string StorageType { get; set; } = string.Empty;

        [JsonPropertyName("bucket")]
        public string Bucket { get; set; } = string.Empty;

        [JsonPropertyName("key")]
        public string Key { get; set; } = string.Empty;
    }
}
