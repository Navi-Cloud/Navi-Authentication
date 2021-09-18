using System;
using System.Collections.Generic;
using MongoDB.Driver;
using NaviAuthService.Model;
using NaviAuthService.Repository;
using NaviAuthServiceTest.TestHelper;
using Xunit;

namespace NaviAuthServiceTest.Repositories
{
    public class UserRepositoryTest: MongoHelper, IDisposable
    {
        private readonly IUserRepository _userRepository;
        private readonly User _mockUser = new()
        {
            UserEmail = "kangdroid@uniquegood.biz",
            UserPassword = "testPassword@",
            UserAccessTokens = new List<AccessToken>()
        };
        
        public void Dispose()
        {
            DestroyDatabase();
        }

        public UserRepositoryTest()
        {
            _userRepository = new UserRepository(_mongoContext);
        }

        [Fact(DisplayName = "RegisterUserAsync: RegisterUserAsync should save user data well.")]
        public async void Is_RegisterUserAsync_Should_Save_Well()
        {
            var result = await _userRepository.RegisterUserAsync(_mockUser);
            Assert.NotNull(result);
            Assert.Equal(_mockUser, result);
        }

        [Fact(DisplayName =
            "RegisterUserAsync: RegisterUserAsync should throw MongoWriteException when duplicated Email Exists.")]
        public async void Is_RegisterUserAsync_Throws_MongoWriteException_When_Duplicated_Email()
        {
            // Register Once
            await _mongoContext._MongoDatabase.GetCollection<User>(nameof(User))
                .InsertOneAsync(_mockUser);
            
            // Do
            await Assert.ThrowsAsync<MongoWriteException>(() => _userRepository.RegisterUserAsync(_mockUser));
        }

        [Fact(DisplayName = "FindUserByEmailAsync: FindUserByEmailAsync should return correct entity if exists")]
        public async void Is_FindUserByEmailAsync_Returns_Non_Null_Entity()
        {
            // Register Once
            await _mongoContext._MongoDatabase.GetCollection<User>(nameof(User))
                .InsertOneAsync(_mockUser);

            var result = await _userRepository.FindUserByEmailAsync(_mockUser.UserEmail);
            
            Assert.NotNull(result);
            Assert.Equal(_mockUser.UserEmail, result.UserEmail);
        }
        
        [Fact(DisplayName = "FindUserByEmailAsync: FindUserByEmailAsync should return null entity if not exists")]
        public async void Is_FindUserByEmailAsync_Returns_Null_Entity()
        {
            var result = await _userRepository.FindUserByEmailAsync(_mockUser.UserEmail);
            
            Assert.Null(result);
        }

        [Fact(DisplayName = "AddAccessTokenToUserAsync: AddAccessTokenToUserAsync should add accesstoken well.")]
        public async void Is_AddAccessTokenToUserAsync_Registers_AccessToken_Well()
        {
            // Register Once
            await _mongoContext._MongoDatabase.GetCollection<User>(nameof(User))
                .InsertOneAsync(_mockUser);
            var mockAccessToken = new AccessToken
            {
                CreatedAt = DateTime.Now,
                ExpiresAt = DateTime.MaxValue,
                Token = ""
            };
            
            // do
            await _userRepository.AddAccessTokenToUserAsync(_mockUser.UserEmail, mockAccessToken);
            
            // Check
            var entityInfo = await _mongoContext._MongoDatabase.GetCollection<User>(nameof(User))
                .FindAsync(a => a.UserEmail == _mockUser.UserEmail);
            var currentEntity = entityInfo.Single();
            Assert.Single(currentEntity.UserAccessTokens);
            Assert.Equal(mockAccessToken.Token, currentEntity.UserAccessTokens[0].Token);
        }

        [Fact(DisplayName =
            "FindUserByAccessTokenAsync: FindUserByAccessTokenAsync should return user entity when accesstoken is found.")]
        public async void Is_FindUserByAccessTokenAsync_Returns_Entity_Well()
        {
            // Register Once
            await _mongoContext._MongoDatabase.GetCollection<User>(nameof(User))
                .InsertOneAsync(_mockUser);
            var mockAccessToken = new AccessToken
            {
                CreatedAt = DateTime.Now,
                ExpiresAt = DateTime.MaxValue,
                Token = "test"
            };
            await _userRepository.AddAccessTokenToUserAsync(_mockUser.UserEmail, mockAccessToken);
            
            // do
            var result = await _userRepository.FindUserByAccessTokenAsync(mockAccessToken.Token);
            
            // Check
            Assert.NotNull(result);
            Assert.NotNull(result.UserAccessTokens);
            Assert.Single(result.UserAccessTokens);
            Assert.Equal(mockAccessToken.Token, result.UserAccessTokens[0].Token);
            Assert.Equal(_mockUser.UserEmail, result.UserEmail);
        }

        [Fact(DisplayName =
            "FindUserByAccessTokenAsync: FindUserByAccessTokenAsync should return null entity when accessToken does not exists.")]
        public async void Is_FindUserByAccessTokenAsync_Returns_Null_When_Not_Found()
        {
            // do
            var result = await _userRepository.FindUserByAccessTokenAsync("mockAccessToken.Token");
            
            // Check
            Assert.Null(result);
        }

        [Fact(DisplayName = "RemoveUserByEmailId: RemoveUserByEmailId should remove user well.")]
        public async void Is_RemoveUserByEmailId_Removes_User_Well()
        {
            // Register Once
            await _mongoContext._MongoDatabase.GetCollection<User>(nameof(User))
                .InsertOneAsync(_mockUser);

            // Do
            await _userRepository.RemoveUserByEmailId(_mockUser.UserEmail);
            
            // Check
            var result = await _userRepository.FindUserByEmailAsync(_mockUser.UserEmail);
            Assert.Null(result);
        }
    }
}