using System;
using System.Diagnostics;
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

            const int messagesToSend = 11000;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            // Pumping messages in the queue
            for (var i = 0; i < messagesToSend; i++)
            {
                SendMessage(new Payment { AmountToPay = 25.0m + i + 1, CardNumber = "11", Name = i + GenRandomName() });
            }

            stopwatch.Stop();

            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("======================================================================");
            Console.WriteLine("Message Sent :: " + messagesToSend);
            Console.WriteLine("Time taken to send messages :: " + stopwatch.Elapsed.Seconds);
            Console.WriteLine("======================================================================");
        }

        private static string GenRandomName()
        {
            return Guid.NewGuid().ToString();
        }

        private static void SendMessage(Payment message)
        {
            _model.BasicPublish("", QueueName, null, message.Serialize());
            Console.WriteLine("Message on Queue :: {0} : {1}", message.CardNumber, message.AmountToPay);
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
