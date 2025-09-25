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
        void PublishMessage<T>(string queueName, T message) where T : class;
        void SubscribeToQueue<T>(string queueName, Func<T, Task> onMessageReceived) where T : class;
        void CreateQueue(string queueName, bool durable = true);
        void CloseConnection();
    }
    
    public class RabbitMQService : IRabbitMQService, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
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
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
                _logger.LogInformation("Conexão com RabbitMQ estabelecida com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao conectar com RabbitMQ");
                throw;
            }
        }
        
        public void CreateQueue(string queueName, bool durable = true)
        {
            try
            {
                _channel.QueueDeclare(
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
        
        public void PublishMessage<T>(string queueName, T message) where T : class
        {
            try
            {
                CreateQueue(queueName);
                
                var messageJson = JsonSerializer.Serialize(message);
                var body = Encoding.UTF8.GetBytes(messageJson);
                
                var properties = _channel.CreateBasicProperties();
                properties.Persistent = true;
                properties.MessageId = Guid.NewGuid().ToString();
                properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                
                _channel.BasicPublish(
                    exchange: "",
                    routingKey: queueName,
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
        
        public void SubscribeToQueue<T>(string queueName, Func<T, Task> onMessageReceived) where T : class
        {
            try
            {
                CreateQueue(queueName);
                
                var consumer = new EventingBasicConsumer(_channel);
                consumer.Received += async (model, ea) =>
                {
                    try
                    {
                        var body = ea.Body.ToArray();
                        var messageJson = Encoding.UTF8.GetString(body);
                        var message = JsonSerializer.Deserialize<T>(messageJson);
                        
                        if (message != null)
                        {
                            await onMessageReceived(message);
                            _channel.BasicAck(ea.DeliveryTag, false);
                            _logger.LogInformation($"Mensagem processada com sucesso da fila '{queueName}': {messageJson}");
                        }
                        else
                        {
                            _logger.LogWarning($"Mensagem nula recebida da fila '{queueName}'");
                            _channel.BasicNack(ea.DeliveryTag, false, false);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Erro ao processar mensagem da fila '{queueName}'");
                        _channel.BasicNack(ea.DeliveryTag, false, true);
                    }
                };
                
                _channel.BasicConsume(
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
        
        public void CloseConnection()
        {
            try
            {
                _channel?.Close();
                _connection?.Close();
                _logger.LogInformation("Conexão com RabbitMQ fechada");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao fechar conexão com RabbitMQ");
            }
        }
        
        public void Dispose()
        {
            CloseConnection();
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}