using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FetchApiTutorial.Models.User
{
    public class RefreshToken
    {
        [BsonId, BsonRepresentation(BsonType.ObjectId)]public ObjectId Id { get; set; }
        public string Token { get; set; }
        [BsonRepresentation(BsonType.DateTime)]public DateTime Expires { get; set; }
        [BsonRepresentation(BsonType.DateTime)]public DateTime Created { get; set; }
        public string CreatedByIp { get; set; }
        [BsonRepresentation(BsonType.DateTime)]public DateTime? Revoked { get; set; }
        public string RevokedByIp { get; set; }
        public string ReplacedByToken { get; set; }
        public string ReasonRevoked { get; set; }
        public bool IsExpired => DateTime.UtcNow >= Expires;
        public bool IsRevoked => Revoked != null;
        public bool IsActive => !IsRevoked && !IsExpired;
    }
}
