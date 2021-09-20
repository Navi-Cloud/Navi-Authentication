using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Grpc.Core;
using Io.Github.NaviCloud.Shared;
using Io.Github.NaviCloud.Shared.Authentication;
using MongoDB.Driver;
using NaviAuthService.Model;
using NaviAuthService.Repository;

namespace NaviAuthService.Service
{
    public class UserService: Authentication.AuthenticationBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IKafkaIntegration _kafkaIntegration;
        private readonly IStorageIntegration _storageIntegration;

        public UserService(IUserRepository userRepository, IKafkaIntegration kafkaIntegration, IStorageIntegration storageIntegration)
        {
            _userRepository = userRepository;
            _kafkaIntegration = kafkaIntegration;
            _storageIntegration = storageIntegration;
        }
        
        /// <summary>
        /// Register User!
        /// </summary>
        /// <param name="request">Registration Request from Body.</param>
        /// <param name="context">Context - to be injected automatically by RPC Provider.</param>
        /// <returns></returns>
        public override async Task<Result> RegisterUser(RegisterRequest request, ServerCallContext context)
        {
            // Create User from Request
            var user = new User(request);
            
            // Try to save User
            try
            {
                await _userRepository.RegisterUserAsync(user);
            }
            catch (Exception superException)
            {
                return HandleRegisterError(superException, user);
            }
            
            // Notify storage service to create root folder for new user.
            await _storageIntegration.RequestRootFolderCreation(user.UserEmail);

            return new Result { ResultType = ResultType.Success };
        }
        
        /// <summary>
        /// Log-In User!
        /// </summary>
        /// <param name="request">The User Login Request</param>
        /// <param name="context">Context - to be injected automatically by RPC Provider.</param>
        /// <returns></returns>
        public override async Task<Result> LoginUser(LoginRequest request, ServerCallContext context)
        {
            // Find User with Id
            var userEntity = await _userRepository.FindUserByEmailAsync(request.UserEmail);

            if (userEntity?.CheckPassword(request.UserPassword) is false or null)
            {
                return new Result
                {
                    ResultType = ResultType.Forbidden,
                    Message = "Id or password is wrong!",
                    Object = ""
                };
            }

                // If Matches - Create Access Token and register to user[update user db]
            var accessToken = GenerateToken(userEntity.UserEmail);
            await _userRepository.AddAccessTokenToUserAsync(userEntity.UserEmail, accessToken);

            return new Result
            {
                ResultType = ResultType.Success,
                Message = "",
                Object = JsonSerializer.Serialize(accessToken)
            };
        }
        
        /// <summary>
        /// Authenticate User - Authenticate User from Access Token
        /// </summary>
        /// <param name="request">Access Token to validate</param>
        /// <param name="context">Context - To be injected from RPC Provider.</param>
        /// <returns>Result Object</returns>
        public override async Task<Result> AuthenticateUser(AuthenticationRequest request, ServerCallContext context)
        {
            // Get Authentication Access Token
            // Search it through User DB
            var userEntity = await _userRepository.FindUserByAccessTokenAsync(request.UserAccessToken);

            if (userEntity == null)
            {
                return new Result
                {
                    ResultType = ResultType.Forbidden,
                    Object = "",
                    Message = "Authentication Failed! Please login."
                };
            }

            return new Result
            {
                ResultType = ResultType.Success,
                Message = "",
                Object = userEntity.UserEmail
            };
        }
        
        /// <summary>
        /// Remove User from User DB
        /// </summary>
        /// <param name="request">User Email</param>
        /// <param name="context">Context - To be injected from RPC Provider.</param>
        /// <returns></returns>
        public override async Task<Result> RemoveUser(AccountRemovalRequest request, ServerCallContext context)
        {
            // Email is verified by now
            // Remove User-Related thingy from user-db.
            await _userRepository.RemoveUserByEmailId(request.UserEmail);
            
            // Send User Removal request on Storage Service.[Queue]
            await _kafkaIntegration.SendRemovalRequest(request.UserEmail);

            return new Result
            {
                ResultType = ResultType.Success,
                Message = "",
                Object = ""
            };
        }

        /// <summary>
        /// Generate SHA-512 Token based on User Email && current time[With CPU Ticks]
        /// </summary>
        /// <param name="userEmail">User who are trying to log-in</param>
        /// <returns>Access Token Object.</returns>
        private AccessToken GenerateToken(string userEmail)
        {
            var concatString = $"{userEmail}/{DateTime.Now.Ticks}";
            using var shaManager = new SHA512Managed();
            var hashValue = shaManager.ComputeHash(Encoding.UTF8.GetBytes(concatString));
            
            return new AccessToken
            {
                CreatedAt = DateTime.Now,
                ExpiresAt = DateTime.Now.AddDays(1),
                Token = BitConverter.ToString(hashValue).Replace("-", "").ToLower()
            };
        }

        /// <summary>
        /// Handle <see cref="RegisterUserAsync"/>'s exception if required. 
        /// </summary>
        /// <param name="superException">Master Exception[Supertype Exception]</param>
        /// <param name="toRegister">User entity tried to register.</param>
        /// <returns>See <see cref="IUserService.RegisterUserAsync"/> for more details.</returns>
        [ExcludeFromCodeCoverage]
        private Result HandleRegisterError(Exception superException, User toRegister)
        {
            // When Error type is MongoWriteException
            if (superException is MongoWriteException mongoWriteException)
            {
                // When Error Type is 'Duplicate Key'
                if (mongoWriteException.WriteError.Category == ServerErrorCategory.DuplicateKey)
                {
                    return new Result
                    {
                        ResultType = ResultType.Duplicate,
                        Message = $"User Email {toRegister.UserEmail} already exists!",
                        Object = ""
                    };
                } // Else -> goto Unknown Error.
            }

            // Unknown if exception is not MongoWriteException.
            return new Result
            {
                ResultType = ResultType.Unknown,
                Message = $"Unknown Error Occurred! : {superException.Message}",
                Object = ""
            };
        }
    }
}