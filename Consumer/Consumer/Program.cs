using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;

namespace Consumer
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var brokerList = "localhost:9092";
            var topics = new List<string> { "example" };
            Console.WriteLine($"Started consumer, Ctrl-C to stop consuming");

            CancellationTokenSource cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) =>

            {
                e.Cancel = true; 
                cts.Cancel();
            };

            Run_ManualAssign(brokerList, topics, cts.Token);

        }

        public static void Run_ManualAssign(string broker, List<string> topics, CancellationToken cancellationToken)
        {
            var config = new ConsumerConfig
            {
                GroupId = new Guid().ToString(),
                BootstrapServers = broker,
                EnableAutoCommit = true
            };

            using (var consumer =
                new ConsumerBuilder<Ignore, string>(config)
                    .SetErrorHandler((_, e) => Console.WriteLine($"Error: {e.Reason}"))
                    .Build())
            {
                consumer.Assign(topics.Select(topic => new TopicPartitionOffset(topic, 0, Offset.Beginning)).ToList());

                try
                {
                    while (true)
                    {
                        try
                        {
                            var consumeResult = consumer.Consume(cancellationToken);
                            Console.WriteLine($"Received message at {consumeResult.TopicPartitionOffset}: ${consumeResult.Value}");
                        }
                        catch (ConsumeException e)
                        {
                            Console.WriteLine($"Consume error: {e.Error.Reason}");
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Closing consumer.");
                    consumer.Close();
                }
            }
        }
    }
}
