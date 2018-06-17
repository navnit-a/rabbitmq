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
            var t = new Thread(MyThreadMethod) {IsBackground = true};
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

            while (true)
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                var ea = consumer.Queue.Dequeue();
                var message = (Payment) ea.Body.DeSerialize(typeof(Payment));
                Console.WriteLine("---- Received {0} : ", message.Name);

                var sqlStuff = new SqlStuff();
                await sqlStuff.SaveStuff(message);

                _model.BasicAck(ea.DeliveryTag, false); // send a message to RabbitMQ server, saying you can discard it now
                stopwatch.Stop();
                Console.WriteLine("Elapsed :: " + stopwatch.Elapsed.TotalSeconds);
            }
        }
    }
}