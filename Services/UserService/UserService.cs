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
            _context = (MongoDatabase)context;
            _jwtUtils = jwtUtils;
        }

        public async Task<AuthenticateResponse> Authenticate(AuthenticationRequest model, string ipAddress)
        {
            var user = await _context.Users.Find(t => t.Username == model.Username && t.Password == model.Password)
                .SingleOrDefaultAsync();
            if (user == null) return null;


            var token = _jwtUtils.GenerateJwtToken(user);
            var refreshToken = _jwtUtils.GenerateRefreshToken(ipAddress);

            await AddToRefreshTokens(user, refreshToken);
            await RemoveOldRefreshTokens(user);

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
            var refreshToken = await GetRefreshToken(token);



            if (refreshToken == null && user == null)
                return null;

            if (refreshToken.Revoked != null)
            {
                // revoke all descendant tokens in case this token has been compromised
                RevokeDescendantRefreshTokens(refreshToken, user, ipAddress,
                    $"Attempted reuse of revoked ancestor token: {token}");
            }

            try
            {
                if (!refreshToken.IsActive)
                    return null;
            }
            catch (Exception)
            {
            }




            // replace old refresh token with a new one (rotate token)
            var newRefreshToken = await RotateRefreshToken(user, refreshToken, ipAddress);
            await AddToRefreshTokens(user, newRefreshToken);

            // remove old refresh tokens from user
            await RemoveOldRefreshTokens(user);

            await UpdateUser(user);

            // generate new jwt
            var jwtToken = _jwtUtils.GenerateJwtToken(user);

            return new AuthenticateResponse(user, jwtToken, newRefreshToken.Token);
        }

        public async Task<RefreshToken> RevokeToken(string token, string ipAddress)
        {
            var user = GetUserByRefreshToken(token);
            if (user == null) return null;

            var refreshToken = await GetRefreshToken(token);


            if (!refreshToken.IsActive)
                return null;

            // revoke token and save
            return await RevokeRefreshToken(user, refreshToken, ipAddress, "Revoked without replacement");
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
            var filter = Builders<MyUser>.Filter.ElemMatch(u => u.RefreshTokens, x => x.Token == token);
            var user = _context.Users.Find(filter).SingleOrDefault();
            return user;
        }

        private async Task<RefreshToken> RotateRefreshToken(MyUser user, RefreshToken refreshToken, string ipAddress)
        {
            var newRefreshToken = _jwtUtils.GenerateRefreshToken(ipAddress);
            await RevokeRefreshToken(user, refreshToken, ipAddress, "Replaced by new token", newRefreshToken.Token);
            return newRefreshToken;
        }

        private async Task RemoveOldRefreshTokens(MyUser user)
        {
            // remove old inactive refresh tokens from user based on TTL in app settings
            var update = Builders<MyUser>.Update.PullFilter(u => u.RefreshTokens, rt => rt.Revoked != null &&
                  rt.Created > DateTime.UtcNow.Subtract(TimeSpan.FromDays(_jwtSettings.RefreshTokenTTL)));

            await _context.Users.FindOneAndUpdateAsync(u => u.Id == user.Id, update);
        }

        private async void RevokeDescendantRefreshTokens(RefreshToken refreshToken, MyUser user, string ipAddress,
            string reason)
        {
            // recursively traverse the refresh token chain and ensure all descendants are revoked
            if (!string.IsNullOrEmpty(refreshToken.ReplacedByToken))
            {
                var childToken = await GetRefreshToken(refreshToken.ReplacedByToken);
                if (childToken != null && childToken.IsActive)
                    await RevokeRefreshToken(user, childToken, ipAddress, reason);
                else
                    RevokeDescendantRefreshTokens(childToken, user, ipAddress, reason);
            }
        }

        private async Task<RefreshToken> RevokeRefreshToken(MyUser user, RefreshToken token, string ipAddress, string reason = null,
            string replacedByToken = null)
        {

            var filter = Builders<MyUser>.Filter.Eq(x => x.Id, user.Id) &
                         Builders<MyUser>.Filter.ElemMatch(x => x.RefreshTokens, Builders<RefreshToken>.Filter.Eq(t => t.Id, token.Id));

            var update = Builders<MyUser>.Update
                .Set(u => u.RefreshTokens[-1].Revoked, DateTime.UtcNow)
                .Set(u => u.RefreshTokens[-1].RevokedByIp, ipAddress)
                .Set(u => u.RefreshTokens[-1].ReasonRevoked, reason)
                .Set(u => u.RefreshTokens[-1].ReplacedByToken, replacedByToken);

            return (await _context.Users.FindOneAndUpdateAsync(filter, update))?.RefreshTokens.Find(t => t.Id == token.Id);

        }

        private async Task UpdateUser(MyUser user)
        {
            var update = Builders<MyUser>.Update.PullFilter(u => u.RefreshTokens, rt => rt.Revoked == null && DateTime.UtcNow < rt.Expires);
            await _context.Users.FindOneAndUpdateAsync(u => u.Id == user.Id, update);
        }

        private async Task<RefreshToken> GetRefreshToken(string token)
        {
            var filter = Builders<MyUser>.Filter.ElemMatch(x => x.RefreshTokens, x => x.Token == token);
            MyUser user = await _context.Users.Find(filter).SingleOrDefaultAsync();
            if (user != null && user.RefreshTokens.Any())
                return user.RefreshTokens.Find(t => t.Token == token);

            return null;
        }

        private async Task AddToRefreshTokens(MyUser user, RefreshToken newRefreshToken)
        {
            var filter = Builders<MyUser>.Filter.Eq(u => u.Id, user.Id);
            var update = Builders<MyUser>.Update.Push(u => u.RefreshTokens, newRefreshToken);
            await _context.Users.UpdateOneAsync(filter, update);
        }
    }
}

