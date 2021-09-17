using System.Threading.Tasks;
using NaviAuthService.Model;

namespace NaviAuthService.Repository
{
    public interface IUserRepository
    {
        /// <summary>
        /// 사용자 정보를 DB에 저장합니다.
        /// </summary>
        /// <param name="user">저장할 사용자 정보</param>
        /// <returns>
        /// <para>저장이 완료된 사용자 정보를 반환합니다.</para>
        /// <para>쓰기작업이 실패하는 경우 MongoWriteException을 던집니다.</para>
        /// </returns>
        public Task<User> RegisterUserAsync(User user);
        
        /// <summary>
        /// Find User Entity by Email
        /// </summary>
        /// <param name="userEmail">User Email Key to search</param>
        /// <returns>User Entity or null if not found.</returns>
        public Task<User> FindUserByEmailAsync(string userEmail);
        
        /// <summary>
        /// Add Access Token to user-embedded documents.
        /// </summary>
        /// <param name="userEmail">User Identifier</param>
        /// <param name="createdToken">The token to insert</param>
        public Task AddAccessTokenToUserAsync(string userEmail, AccessToken createdToken);
        
        /// <summary>
        /// Find User Entity by Access Token Field.
        /// </summary>
        /// <param name="userToken">A Search field[User token]</param>
        /// <returns>User Entity if corresponding entity exists, or null.</returns>
        public Task<User> FindUserByAccessTokenAsync(string userToken);
        
        /// <summary>
        /// Remove User by Email
        /// </summary>
        /// <param name="userEmail">User Identifier</param>
        public Task RemoveUserByEmailId(string userEmail);
    }
}