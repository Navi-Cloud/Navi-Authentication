using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AuthenticationServiceTest.TestHelper;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using NaviAuthService;

namespace NaviAuthServiceTest.TestHelper
{
    public sealed class TestServerFixture : IDisposable
    {
        private readonly WebApplicationFactory<Startup> _webApplicationFactory;
        public GrpcChannel Channel { get; }

        public TestServerFixture()
        {
            _webApplicationFactory = new WebApplicationFactory<Startup>().WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, conf) =>
                {
                    conf.AddJsonStream(TestConfiguration.GetTestConfigurationStream());
                });
                builder.ConfigureTestServices(service =>
                {
                    // var currentDescriptor = service.SingleOrDefault(a => a.ServiceType == typeof(ILoggerService));
                    // service.Remove(currentDescriptor);
                    // service.AddSingleton(provider => new Mock<ILoggerService>().Object);
                });
            });
            var client = _webApplicationFactory.CreateDefaultClient(new ResponseVersionHandler());
            Channel = GrpcChannel.ForAddress(client.BaseAddress, new GrpcChannelOptions
            {
                HttpClient = client
            });
        }

        public void Dispose()
        {
            _webApplicationFactory.Dispose();
        }
        
        private class ResponseVersionHandler : DelegatingHandler
        {
            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                var response = await base.SendAsync(request, cancellationToken);
                response.Version = request.Version;
                return response;
            }
        }
    }
}