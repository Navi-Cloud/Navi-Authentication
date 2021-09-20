using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Grpc.Net.Client;
using Io.Github.NaviCloud.Shared.Storage;
using Microsoft.Extensions.Configuration;
using NaviStorageService;

namespace NaviAuthService.Service
{
    public interface IStorageIntegration
    {
        Task RequestRootFolderCreation(string userEmail);
    }
    
    [ExcludeFromCodeCoverage]
    public class StorageIntegration : IStorageIntegration
    {
        private readonly Folder.FolderClient _folderClient;
        
        public StorageIntegration(IConfiguration configuration)
        {
            _folderClient =
                new Folder.FolderClient(GrpcChannel.ForAddress(configuration.GetConnectionString("StorageConnection")));
        }

        public async Task RequestRootFolderCreation(string userEmail)
        {
            await _folderClient.CreateRootFolderAsync(new CreateRootFolderRequest
            {
                UserEmail = userEmail
            });
        }
    }
}