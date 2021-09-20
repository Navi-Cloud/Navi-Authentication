using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;

namespace NaviAuthService.Extension
{
    [ExcludeFromCodeCoverage]
    public static class NaviExtension
    {
        public static string GetKafkaConfiguration(this IConfiguration configuration, string name)
        {
            return configuration?.GetSection("KafkaStrings")?[name];
        }
    }
}