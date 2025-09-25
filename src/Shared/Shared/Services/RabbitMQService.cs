using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Shared.Services
{
    public interface IRabbitMQService
    {
        Task PublishMessageAsync<T>(string queueName, T message) where T : class;
        Task SubscribeToQueueAsync<T>(string queueName, Func<T, Task> onMessageReceived) where T : class;
        Task CreateQueueAsync(string queueName, bool durable = true);
        Task CloseConnectionAsync();
    }
    
    public class RabbitMQService : IRabbitMQService, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IChannel _channel;
        private readonly ILogger<RabbitMQService> _logger;
        
        public RabbitMQService(IConfiguration configuration, ILogger<RabbitMQService> logger)
        {
            _logger = logger;
            
            var factory = new ConnectionFactory()
            {
                HostName = configuration["RabbitMQ:HostName"] ?? "localhost",
                UserName = configuration["RabbitMQ:UserName"] ?? "guest",
                Password = configuration["RabbitMQ:Password"] ?? "guest",
                Port = int.Parse(configuration["RabbitMQ:Port"] ?? "5672"),
                VirtualHost = configuration["RabbitMQ:VirtualHost"] ?? "/"
            };
            
            try
            {
                _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
                _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();
                _logger.LogInformation("Conexão com RabbitMQ estabelecida com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao conectar com RabbitMQ");
                throw;
            }
        }
        
        public async Task CreateQueueAsync(string queueName, bool durable = true)
        {
            try
            {
                await _channel.QueueDeclareAsync(
                    queue: queueName,
                    durable: durable,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);
                
                _logger.LogInformation($"Fila '{queueName}' criada/verificada com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao criar/verificar fila '{queueName}'");
                throw;
            }
        }
        
        public async Task PublishMessageAsync<T>(string queueName, T message) where T : class
        {
            try
            {
                await CreateQueueAsync(queueName);
                
                var messageJson = JsonSerializer.Serialize(message);
                var body = Encoding.UTF8.GetBytes(messageJson);
                
                var properties = new BasicProperties
                {
                    Persistent = true,
                    MessageId = Guid.NewGuid().ToString(),
                    Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                };
                
                await _channel.BasicPublishAsync(
                    exchange: "",
                    routingKey: queueName,
                    mandatory: false,
                    basicProperties: properties,
                    body: body);
                
                _logger.LogInformation($"Mensagem publicada na fila '{queueName}': {messageJson}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao publicar mensagem na fila '{queueName}'");
                throw;
            }
        }
        
        public async Task SubscribeToQueueAsync<T>(string queueName, Func<T, Task> onMessageReceived) where T : class
        {
            try
            {
                await CreateQueueAsync(queueName);
                
                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.ReceivedAsync += async (model, ea) =>
                {
                    try
                    {
                        var body = ea.Body.ToArray();
                        var messageJson = Encoding.UTF8.GetString(body);
                        var message = JsonSerializer.Deserialize<T>(messageJson);
                        
                        if (message != null)
                        {
                            await onMessageReceived(message);
                            await _channel.BasicAckAsync(ea.DeliveryTag, false);
                            _logger.LogInformation($"Mensagem processada com sucesso da fila '{queueName}': {messageJson}");
                        }
                        else
                        {
                            _logger.LogWarning($"Mensagem nula recebida da fila '{queueName}'");
                            await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Erro ao processar mensagem da fila '{queueName}'");
                        await _channel.BasicNackAsync(ea.DeliveryTag, false, true);
                    }
                };
                
                await _channel.BasicConsumeAsync(
                    queue: queueName,
                    autoAck: false,
                    consumer: consumer);
                
                _logger.LogInformation($"Inscrito na fila '{queueName}' com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao se inscrever na fila '{queueName}'");
                throw;
            }
        }
        
        public async Task CloseConnectionAsync()
        {
            try
            {
                if (_channel != null)
                    await _channel.CloseAsync();
                if (_connection != null)
                    await _connection.CloseAsync();
                _logger.LogInformation("Conexão com RabbitMQ fechada");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao fechar conexão com RabbitMQ");
            }
        }
        
        public void Dispose()
        {
            CloseConnectionAsync().GetAwaiter().GetResult();
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}
