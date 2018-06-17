using System;
using Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

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
            Receive();
            Console.ReadLine();
        }

        private static void Receive()
        {
            _connectionFactory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "admin",
                Password = "admin"
            };

            using (_connection = _connectionFactory.CreateConnection())
            {
                using (_model = _connection.CreateModel())
                {
                    _model.QueueDeclare(QueueName, true, false, false, null);
                    _model.BasicQos(0, 1, false);

                    var consumer = new QueueingBasicConsumer(_model);
                    _model.BasicConsume(QueueName, false, consumer);

                    while (true)
                    {
                        var ea = consumer.Queue.Dequeue();
                        var message = (Payment) ea.Body.DeSerialize(typeof(Payment));
                        _model.BasicAck(ea.DeliveryTag, false); // send a message to RabbitMQ server, saying you can discard it now
                        Console.WriteLine("---- Received {0} : {1} : {2} ", message.Name, message.CardNumber,
                            message.AmountToPay);
                    }
                }
            }
        }
    }
}