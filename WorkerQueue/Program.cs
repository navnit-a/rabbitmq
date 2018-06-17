using System;
using System.Threading;
using Common;
using RabbitMQ.Client;

namespace WorkerQueue
{
    public class Program
    {
        private const string QueueName = "WorkerQueue_ExampleQueue";
        private static ConnectionFactory _connectionFactory;
        private static IConnection _connection;
        private static IModel _model;

        static void Main(string[] args)
        {
            CreateConnection();
            
            for (int i = 0; i < 10; i++)
            {
                SendMessage(new Payment { AmountToPay = 25.0m, CardNumber = "1234123412341234", Name = i.ToString()});
                Thread.Sleep(3000);
            }
        }

        private static void SendMessage(Payment message)
        {
            _model.BasicPublish("", QueueName, null, message.Serialize());
            Console.WriteLine("Payment Sent :: {0} : {1}", message.CardNumber, message.AmountToPay);
        }

        private static void CreateConnection()
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
