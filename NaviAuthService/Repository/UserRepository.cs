using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using NaviAuthService.Model;

namespace NaviAuthService.Repository
{
    public class UserRepository: IUserRepository
    {
        private readonly IMongoCollection<User> _userCollection;

        public UserRepository(MongoContext context)
        {
            _userCollection = context._MongoDatabase.GetCollection<User>(nameof(User));
            
            // Setup Index
            _userCollection.Indexes.CreateOne(
                new CreateIndexModel<User>(
                    new BsonDocument { {"userEmail", 1}}, 
                    new CreateIndexOptions { Unique = true })
            );
        }
        
        /// <summary>
        /// See <see cref="IUserRepository.RegisterUserAsync"/> for more details.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<User> RegisterUserAsync(User user)
        {
            // Register User
            await _userCollection.InsertOneAsync(user);
            return user;
        }
        
        /// <summary>
        /// See <see cref="IUserRepository.FindUserByEmailAsync"/> for more details.
        /// </summary>
        /// <param name="userEmail"></param>
        /// <returns></returns>
        public async Task<User> FindUserByEmailAsync(string userEmail)
        {
            var result = await _userCollection.FindAsync(a => a.UserEmail == userEmail);

            return result.SingleOrDefault();
        }
        
        /// <summary>
        /// See <see cref="IUserRepository.AddAccessTokenToUserAsync"/> for more details.
        /// </summary>
        /// <param name="userEmail"></param>
        /// <param name="createdToken"></param>
        /// <returns></returns>
        public async Task AddAccessTokenToUserAsync(string userEmail, AccessToken createdToken)
        {
            var filter = Builders<User>.Filter.And(
                Builders<User>.Filter.Eq(a => a.UserEmail, userEmail)
            );
            var update = Builders<User>.Update.Push(a => a.UserAccessTokens, createdToken);

            await _userCollection.UpdateOneAsync(filter, update);
        }
        
        /// <summary>
        /// See <see cref="IUserRepository.FindUserByAccessTokenAsync"/> for more details.
        /// </summary>
        /// <param name="userToken"></param>
        /// <returns></returns>
        public async Task<User> FindUserByAccessTokenAsync(string userToken)
        {
            var filter = Builders<User>.Filter.Eq("userAccessTokens.Token", userToken);
            var result = await _userCollection.FindAsync(filter);

            return result.FirstOrDefault();
        }
        
        /// <summary>
        /// See <see cref="IUserRepository.RemoveUserByEmailId"/>
        /// </summary>
        /// <param name="userEmail">Email Thingy :)</param>
        public async Task RemoveUserByEmailId(string userEmail)
        {
            var filter = Builders<User>.Filter.Eq(a => a.UserEmail, userEmail);
            await _userCollection.DeleteOneAsync(filter);
        }
    }
}