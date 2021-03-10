using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Back_Office
{
    class Program
    {
        public static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { Uri = new Uri("amqp://admin:iamadmin@localhost:5672") };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                
                channel.ExchangeDeclare(exchange: "topic_logs", type: "topic");
                var queueName = channel.QueueDeclare().QueueName;
                
                channel.QueueBind(queue: queueName,
                    exchange: "topic_logs",
                    routingKey: "tour.*");

                Console.WriteLine(" [*] Waiting for messages. To exit press CTRL+C");

                channel.QueueDeclare("Back-Office_Queue", true, false, false, null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var routingKey = ea.RoutingKey;

                    Console.WriteLine(" [x] Received '{0}':'{1}'",
                        routingKey,
                        message);
                };
                channel.BasicConsume(queue: queueName,
                    autoAck: true,
                    consumer: consumer);

                Console.WriteLine(" Press [enter] to exit.");
                BackOfficeQueueConsume();
                Console.ReadLine();
            }
        }

        private static void BackOfficeQueueConsume()
        {
            var factory = new ConnectionFactory() { Uri = new Uri("amqp://admin:iamadmin@localhost:5672") };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {

                channel.ExchangeDeclare(exchange: "topic_logs", type: "topic");
                var queueName = channel.QueueDeclare().QueueName;

                channel.QueueDeclare("Back_Office", true, false, false, null);


                channel.QueueBind(queue: queueName,
                    exchange: "topic_logs",
                    routingKey: "tour.*");

                Console.WriteLine(" [*] Waiting for messages. To exit press CTRL+C");

                channel.QueueDeclare("Back_Office", true, false, false, null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var routingKey = ea.RoutingKey;

                    Console.WriteLine(" [x] Received '{0}':'{1}'",
                        routingKey,
                        message);
                };
                channel.BasicConsume(queue: "Back_Office",
                    autoAck: true,
                    consumer: consumer);

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }
    }
}