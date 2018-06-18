using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Common;
using RabbitMQ.Client;

namespace WorkerQueue_Consumer
{
    public class Program
    {
        private const string QueueName = "WorkerQueue_ExampleQueue";
        private static ConnectionFactory _connectionFactory;
        private static IConnection _connection;
        private static IModel _model;

        private static void Main(string[] args)
        {
            var t = new Thread(MyThreadMethod) { IsBackground = true };
            t.Start();
            Console.WriteLine("The thread's background status is: " + t.IsBackground);
            Console.Read();
        }

        private static async void MyThreadMethod()
        {
            await ReceiveAsync();
        }

        private static async Task ReceiveAsync()
        {
            _connectionFactory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "admin",
                Password = "admin"
            };

            _connection = _connectionFactory.CreateConnection();
            _model = _connection.CreateModel();
            _model.QueueDeclare(QueueName, true, false, false, null);
            _model.BasicQos(0, 1, false);

            var consumer = new QueueingBasicConsumer(_model);
            _model.BasicConsume(QueueName, false, consumer);

            var stopwatch = new Stopwatch();
            var countMessageProcessed = 0;

            while (true)
            {
                var ea = consumer.Queue.Dequeue();
                var messageCount = (int)_model.MessageCount(QueueName);
                Console.WriteLine("Queue Size :: " + messageCount);

                // Waiting for over 10,000 messages to begin processing
                stopwatch.Start();
                var message = (Payment)ea.Body.DeSerialize(typeof(Payment));
                Console.WriteLine("Saving Message to db Received {0} : ", message.Name);

                var sqlStuff = new SqlStuff();
                await sqlStuff.SaveStuff(message);

                _model.BasicAck(ea.DeliveryTag, false); // send a message to RabbitMQ server, saying you can discard it now
                countMessageProcessed++;

                if (countMessageProcessed == 10000)
                {
                    Console.BackgroundColor = ConsoleColor.Blue;
                    Console.ForegroundColor = ConsoleColor.White;
                    stopwatch.Stop();
                    Console.WriteLine("======================================================================");
                    Console.WriteLine("Processed Messages :: " + countMessageProcessed);
                    Console.WriteLine("Elapsed Time :: " + stopwatch.Elapsed.TotalSeconds);
                    Console.WriteLine("Messages processed per seconds :: " + stopwatch.Elapsed.TotalSeconds / 10000);
                    Console.WriteLine("======================================================================");
                    Console.ReadLine();
                }
            }
        }
    }
}