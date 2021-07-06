using FetchApiTutorial.Data;
using FetchApiTutorial.Models.User;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using FetchApiTutorial.Helpers.Settings;

namespace FetchApiTutorial.Services.UserService
{
    public class UserService : IUserService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly MongoDatabase _context;

        public UserService(IOptions<JwtSettings> appSettings, IDatabase context)
        {
            _jwtSettings = appSettings.Value;
            _context = (MongoDatabase)context;
        }

        public async Task<AuthenticateResponse> Authenticate(AuthenticationRequest model)
        {
            var user = await _context.Users.Find(t => t.Username == model.Username && t.Password == model.Password).SingleOrDefaultAsync();
            if (user == null) return null;


            var token = GenerateJwtToken(user);
            return new AuthenticateResponse(user, token);
        }

        public async Task<IEnumerable<MyUser>> GetAllAsync()
        {
            return await _context.Users.Find(new BsonDocument()).ToListAsync(); ;
        }

        public async Task<MyUser> GetByIdAsync(string id)
        {
            var filter = Builders<MyUser>.Filter.Eq(t => t.Id, new ObjectId(id));
            return await _context.Users.Find(filter).SingleOrDefaultAsync();
        }

        public async Task<MyUser> RegisterUserAsync(AuthenticationRequest request)
        {
            MyUser user = new MyUser
            {
                Id = ObjectId.GenerateNewId(),
                Username = request.Username,
                Password = request.Password,
            };

            long count = await _context.Users.Find(t => t.Username == user.Username).CountDocumentsAsync();

            if (count > 0)
                return null;

            await _context.Users.InsertOneAsync(user);
            return user;
        }

        private string GenerateJwtToken(MyUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", user.Id.ToString()) }),
                Expires = DateTime.Now.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
