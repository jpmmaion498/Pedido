using Amazon.S3;
using Amazon.S3.Transfer;
using Amazon.SQS;
using Amazon.SQS.Model;
using Pedido.Domain.Entities;
using Serilog;
using System.Text;
using System.Text.Json;

public class SqsService
{
    private readonly IAmazonSQS _sqsClient;
    private readonly IAmazonS3 _s3Client;
    private readonly string _queueUrl = "http://localhost:4566/000000000000/pedidos.fifo";
    private const int SQS_MAX_SIZE_BYTES = 262144;
    private const string BucketName = "pedidos-arquivos";

    public SqsService(IAmazonSQS sqsClient, IAmazonS3 s3Client)
    {
        _sqsClient = sqsClient;
        _s3Client = s3Client;
    }

    public async Task EnviarParaFilaAsync(PedidoEntity pedido)
    {
        try
        {
            var json = JsonSerializer.Serialize(pedido);
            var size = Encoding.UTF8.GetByteCount(json);

            string messageBody;

            if (size > SQS_MAX_SIZE_BYTES)
            {
                Log.Warning("Pedido {PedidoId} excede 256KB ({Size} bytes). Salvando no S3...", pedido.PedidoId, size);

                var key = $"pedidos/{DateTime.UtcNow:yyyy/MM/dd}/pedido-{pedido.PedidoId}.json";
                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
                var upload = new TransferUtility(_s3Client);
                await upload.UploadAsync(stream, BucketName, key);

                Log.Information("Pedido {PedidoId} salvo no S3: {Key}", pedido.PedidoId, key);

                messageBody = JsonSerializer.Serialize(new
                {
                    storageType = "s3",
                    bucket = BucketName,
                    key = key
                });
            }
            else
            {
                messageBody = json;
                Log.Information("Pedido {PedidoId} dentro do limite de 256KB. Enviando JSON direto para a fila.", pedido.PedidoId);
            }

            var request = new SendMessageRequest
            {
                QueueUrl = _queueUrl,
                MessageBody = messageBody,
                MessageGroupId = "pedidos",
                MessageDeduplicationId = Guid.NewGuid().ToString()
            };

            await _sqsClient.SendMessageAsync(request);
            Log.Information("Pedido {PedidoId} enviado para a fila com sucesso", pedido.PedidoId);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erro ao tentar enviar pedido {PedidoId} para a fila", pedido.PedidoId);
            throw;
        }
    }
}
