using FetchApiTutorial.Data;
using FetchApiTutorial.Models.User;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using FetchApiTutorial.Helpers.Settings;
using FetchApiTutorial.Models.User.RequestsAndResponses;
using FetchApiTutorial.Services.JwtUtils;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson.Serialization;

namespace FetchApiTutorial.Services.UserService
{
    public class UserService : IUserService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly MongoDatabase _context;
        private readonly IJwtUtils _jwtUtils;


        public UserService(IOptions<JwtSettings> appSettings, IDatabase context, IJwtUtils jwtUtils)
        {
            _jwtSettings = appSettings.Value;
            _context = (MongoDatabase) context;
            _jwtUtils = jwtUtils;
        }

        public async Task<AuthenticateResponse> Authenticate(AuthenticationRequest model, string ipAdress)
        {
            var user = await _context.Users.Find(t => t.Username == model.Username && t.Password == model.Password)
                .SingleOrDefaultAsync();
            if (user == null) return null;


            var token = _jwtUtils.GenerateJwtToken(user);
            var refreshToken = _jwtUtils.GenerateRefreshToken(ipAdress);

            user.RefreshTokens.Add(refreshToken);
            RemoveOldRefreshTokens(user);

            await _context.Users.ReplaceOneAsync(x => x.Id == user.Id, user);

            return new AuthenticateResponse(user, token, refreshToken.Token);
        }

        public async Task<IEnumerable<MyUser>> GetAllAsync()
        {
            return await _context.Users.Find(new BsonDocument()).ToListAsync();
        }

        public async Task<MyUser> GetByIdAsync(string id)
        {
            var filter = Builders<MyUser>.Filter.Eq(t => t.Id, new ObjectId(id));
            return await _context.Users.Find(filter).SingleOrDefaultAsync();
        }

        public async Task<AuthenticateResponse> RefreshToken(string token, string ipAddress)
        {
            var user = GetUserByRefreshToken(token);
            var refreshToken = user.RefreshTokens.Single(x => x.Token == token);

            if (refreshToken.IsRevoked)
            {
                // revoke all descendant tokens in case this token has been compromised
                RevokeDescendantRefreshTokens(refreshToken, user, ipAddress,
                    $"Attempted reuse of revoked ancestor token: {token}");
            }

            if (!refreshToken.IsActive)
                return null;

            // replace old refresh token with a new one (rotate token)
            var newRefreshToken = RotateRefreshToken(refreshToken, ipAddress);
            user.RefreshTokens.Add(newRefreshToken);

            // remove old refresh tokens from user
            RemoveOldRefreshTokens(user);

            await _context.Users.ReplaceOneAsync(x => x.Id == user.Id, user);

            // generate new jwt
            var jwtToken = _jwtUtils.GenerateJwtToken(user);

            return new AuthenticateResponse(user, jwtToken, newRefreshToken.Token);
        }

        public async Task RevokeToken(string token, string ipAddress)
        {
            var user = GetUserByRefreshToken(token);
            var refreshToken = user.RefreshTokens.Single(x => x.Token == token);

            if (!refreshToken.IsActive)
                return;

            // revoke token and save
            RevokeRefreshToken(refreshToken, ipAddress, "Revoked without replacement");
            await _context.Users.ReplaceOneAsync(x => x.Id == user.Id, user);
        }

        public async Task<MyUser> RegisterUserAsync(AuthenticationRequest request)
        {
            MyUser user = new MyUser
            {
                Id = ObjectId.GenerateNewId(),
                Username = request.Username,
                Password = request.Password,
                RefreshTokens = new List<RefreshToken>()
            };

            long count = await _context.Users.Find(t => t.Username == user.Username).CountDocumentsAsync();

            if (count > 0)
                return null;

            await _context.Users.InsertOneAsync(user);
            return user;
        }

        private MyUser GetUserByRefreshToken(string token)
        {
            var user = _context.Users.Find(u => u.RefreshTokens.Any(t => t.Token == token)).SingleOrDefault();
            return user;
        }

        private RefreshToken RotateRefreshToken(RefreshToken refreshToken, string ipAddress)
        {
            var newRefreshToken = _jwtUtils.GenerateRefreshToken(ipAddress);
            RevokeRefreshToken(refreshToken, ipAddress, "Replaced by new token", newRefreshToken.Token);
            return newRefreshToken;
        }

        private void RemoveOldRefreshTokens(MyUser user)
        {
            // remove old inactive refresh tokens from user based on TTL in app settings
            user.RefreshTokens.RemoveAll(x =>
                !x.IsActive &&
                x.Created.AddDays(_jwtSettings.RefreshTokenTTL) <= DateTime.UtcNow);
        }

        private void RevokeDescendantRefreshTokens(RefreshToken refreshToken, MyUser user, string ipAddress,
            string reason)
        {
            // recursively traverse the refresh token chain and ensure all descendants are revoked
            if (!string.IsNullOrEmpty(refreshToken.ReplacedByToken))
            {
                var childToken = user.RefreshTokens.SingleOrDefault(x => x.Token == refreshToken.ReplacedByToken);
                if (childToken != null && childToken.IsActive)
                    RevokeRefreshToken(childToken, ipAddress, reason);
                else
                    RevokeDescendantRefreshTokens(childToken, user, ipAddress, reason);
            }
        }

        private void RevokeRefreshToken(RefreshToken token, string ipAddress, string reason = null,
            string replacedByToken = null)
        {
            token.Revoked = DateTime.UtcNow;
            token.RevokedByIp = ipAddress;
            token.ReasonRevoked = reason;
            token.ReplacedByToken = replacedByToken;
        }

        private void UpdateUser()
        {
            //var update = Builders<MyUser>.Update.PullFilter()
        }
    }
}

