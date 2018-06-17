using System;
using System.Threading;
using System.Threading.Tasks;
using Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Rabbit
{
    internal class Program
    {
        private const string QueueName = "StandardQueue_ExampleQueue";
        private static ConnectionFactory _connectionFactory;
        private static IConnection _connection;
        private static IModel _model;

        private static void Main(string[] args)
        {
            var payment1 = new Payment { AmountToPay = 25.0m, CardNumber = "1234123412341234" };
            var payment2 = new Payment { AmountToPay = 5.0m, CardNumber = "1234123412341234" };

            CreateQueue();
            SendMessage(payment1);
            SendMessage(payment2);
            Receive();

            Console.ReadLine();
        }

        private static void Receive()
        {
            var consumer = new EventingBasicConsumer(_model);
            _model.BasicConsume(QueueName, true, consumer);

            consumer.Received += (sender, e) =>
            {
                var message = (Payment)e.Body.DeSerialize(typeof(Payment));

                Console.WriteLine("---- Received {0} : {1} : {2} ", message.Name, message.CardNumber,
                    message.AmountToPay);
            };
        }

        private static uint GetMessageCount(IModel model, string queueName)
        {
            var results = model.QueueDeclare(queueName, true, false, false, null);
            return results.MessageCount;
        }

        private static void SendMessage(Payment message)
        {
            _model.BasicPublish("", QueueName, null, message.Serialize());
            Console.WriteLine($"[x] Payment Message Sent : {0} : {1}",
                message.CardNumber,
                message.AmountToPay);
        }

        private static void CreateQueue()
        {
            _connectionFactory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "admin",
                Password = "admin"
            };

            _connection = _connectionFactory.CreateConnection();

            _model = _connection.CreateModel();

            const bool exclusive = false;
            const bool durable = true;
            const bool autoDelete = false;


            _model.QueueDeclare(QueueName, durable, exclusive, autoDelete, null);
        }
    }
}