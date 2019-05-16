using Confluent.Kafka;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Producer
{
    class Program
    {
        static void Main(string[] args)
        {
            string brokerList = "localhost:9092";
            string topicName = "example";

            var config = new ProducerConfig { BootstrapServers = brokerList };

            using (var producer = new ProducerBuilder<string, string>(config).Build())
            {
                Console.WriteLine($"Producer {producer.Name} producing on topic {topicName}.");
                Console.WriteLine("-----------------------------------------------------------------------");
                Console.WriteLine("> value<enter>");
                Console.WriteLine("Ctrl-C to quit.\n");

                var cancelled = false;
                Console.CancelKeyPress += (_, e) => {
                    e.Cancel = true; 
                    cancelled = true;
                };

                while (!cancelled)
                {
                    Console.Write("> ");

                    string text;
                    try
                    {
                        text = Console.ReadLine();
                    }
                    catch (IOException)
                    {
                        break;
                    }
                    if (text == null)
                    {
                        break;
                    }

                    try
                    {
                            producer.Produce(topicName, new Message<string, string> { Key = "", Value = text });
                    }
                    catch (ProduceException<string, string> e)
                    {
                        Console.WriteLine($"failed to deliver message: {e.Message} [{e.Error.Code}]");
                    }
                }
            }
        }
    }
}
