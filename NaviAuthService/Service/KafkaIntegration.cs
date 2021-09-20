using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using NaviAuthService.Extension;

namespace NaviAuthService.Service
{
    public interface IKafkaIntegration
    {
        Task SendRemovalRequest(string userEmail);
    }
    
    [ExcludeFromCodeCoverage]
    public class KafkaIntegration : IKafkaIntegration
    {
        private readonly ProducerConfig _producerConfig;
        private readonly string _producerTopic;
        
        public KafkaIntegration(IConfiguration configuration)
        {
            _producerConfig = new ProducerConfig
            {
                BootstrapServers = configuration.GetKafkaConfiguration("KafkaServerAddr"),
                ClientId = Dns.GetHostName(),
                Acks = Acks.All
            };

            _producerTopic = configuration.GetKafkaConfiguration("KafkaTopic");
        }

        public async Task SendRemovalRequest(string userEmail)
        {
            using var producer = new ProducerBuilder<Null, string>(_producerConfig).Build();
            await producer.ProduceAsync(_producerTopic, new Message<Null, string> {Value = userEmail});
        }
    }
}