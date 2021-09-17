using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Naviauthentication;

namespace NaviAuthService.Model
{
    /// <summary>
    /// User model description. All about users!
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class User
    {
        /// <summary>
        /// Unique ID[Or Identifier] for Each User.
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        /// <summary>
        /// User's Email Address
        /// </summary>
        [BsonElement("userEmail")]
        public string UserEmail { get; set; }

        /// <summary>
        /// User's Password Information. Note this should be encrypted.
        /// </summary>
        public string UserPassword { get; set; } // TODO: Need to be encrypted.
        
        /// <summary>
        /// User Access Tokens
        /// </summary>
        [BsonElement("userAccessTokens")]
        public List<AccessToken> UserAccessTokens { get; set; }

        /// <summary>
        /// Empty Constructor
        /// </summary>
        public User() { }

        /// <summary>
        /// Create User from UserRegisterRequest
        /// </summary>
        /// <param name="request">UserRegisterRequest from GRPC</param>
        public User(RegisterRequest request)
        {
            UserEmail = request.UserEmail;
            UserPassword = request.UserPassword;
            UserAccessTokens = new List<AccessToken>();
        }

        public bool CheckPassword(string input)
        {
            return UserPassword == input;
        }
    }
}