using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using ProductUser.Microservice.Data;
using ProductUser.Microservice.Model;
using ProductUser.Microservice.Utility;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace ProductUser.Microservice.BackgroundServices
{
    public class RabbitMQBackgroundConsumerService : BackgroundService
    {
        private IConnection _connection;
        private IModel _channel;
        private IServiceScopeFactory serviceScopeFactory;
        private readonly ILogger<RabbitMQBackgroundConsumerService> _logger;

        public RabbitMQBackgroundConsumerService(IServiceScopeFactory _serviceScopeFactory, ILogger<RabbitMQBackgroundConsumerService> logger)
        {
            serviceScopeFactory = _serviceScopeFactory;
            _logger = logger;
            InitRabbitMQ();
        }

        private void InitRabbitMQ()
        {
            var RabbitMQServer = "";
            var RabbitMQUserName = "";
            var RabbitMQPassword = "";

            _logger.LogInformation($"Environment Variable RABBIT_MQ_SERVER: {Environment.GetEnvironmentVariable("RABBIT_MQ_SERVER")}");
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("RABBIT_MQ_SERVER")))
            {
                RabbitMQServer = Environment.GetEnvironmentVariable("RABBIT_MQ_SERVER");
                RabbitMQUserName = Environment.GetEnvironmentVariable("RABBIT_MQ_USERNAME");
                RabbitMQPassword = Environment.GetEnvironmentVariable("RABBIT_MQ_PASSWORD");

            }
            else
            {
                RabbitMQServer = StaticConfigurationManager.AppSetting["RabbitMQ:RabbitURL"];
                RabbitMQUserName = StaticConfigurationManager.AppSetting["RabbitMQ:Username"];
                RabbitMQPassword = StaticConfigurationManager.AppSetting["RabbitMQ:Password"];
            }

            var factory = new ConnectionFactory()
            { HostName = RabbitMQServer, UserName = RabbitMQUserName, Password = RabbitMQPassword };

            // create connection  
            _connection = factory.CreateConnection();

            // create channel  
            _channel = _connection.CreateModel();

            //Direct Exchange Details like name and type of exchange
            _channel.ExchangeDeclare(StaticConfigurationManager.AppSetting["RabbitMqSettings:ExchangeName"], StaticConfigurationManager.AppSetting["RabbitMqSettings:ExchhangeType"]);

            //Declare Queue with Name and a few property related to Queue like durabality of msg, auto delete and many more
            _channel.QueueDeclare(queue: StaticConfigurationManager.AppSetting["RabbitMqSettings:QueueName"],
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null);


            _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            _channel.QueueBind(queue: StaticConfigurationManager.AppSetting["RabbitMqSettings:QueueName"], exchange: StaticConfigurationManager.AppSetting["RabbitMqSettings:ExchangeName"], routingKey: StaticConfigurationManager.AppSetting["RabbitMqSettings:RouteKey"]);

            _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (ch, ea) =>
            {
                // received message  
                var content = System.Text.Encoding.UTF8.GetString(ea.Body.ToArray());

                // acknowledge the received message  
                _channel.BasicAck(ea.DeliveryTag, false);

                //Deserilized Message
                var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                var productDetails = JsonConvert.DeserializeObject<ProductOfferDetail>(message);
                _logger.LogInformation($"Message received from RabbitMQ: {message}");

                //Stored Offer Details into the Database
                using (var scope = serviceScopeFactory.CreateScope())
                {
                    var _dbContext = scope.ServiceProvider.GetRequiredService<DbContextClass>();
                    var result = _dbContext.ProductOffers.Add(productDetails);
                    _dbContext.SaveChanges();
                    _logger.LogInformation($"Message saved to Database: {message}");
                }

            };

            consumer.Shutdown += OnConsumerShutdown;
            consumer.Registered += OnConsumerRegistered;
            consumer.Unregistered += OnConsumerUnregistered;
            consumer.ConsumerCancelled += OnConsumerConsumerCancelled;

            _channel.BasicConsume(StaticConfigurationManager.AppSetting["RabbitMqSettings:QueueName"], false, consumer);
            return Task.CompletedTask;
        }

        private void OnConsumerConsumerCancelled(object sender, ConsumerEventArgs e) { }
        private void OnConsumerUnregistered(object sender, ConsumerEventArgs e) { }
        private void OnConsumerRegistered(object sender, ConsumerEventArgs e) { }
        private void OnConsumerShutdown(object sender, ShutdownEventArgs e) { }
        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e) { }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }
    }
}
