using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FetchApiTutorial.Models
{
    public class MyTask
    {
        [BsonId, BsonRepresentation(BsonType.ObjectId), BsonRequired] public ObjectId Id { get; set; }
        [BsonRequired] public string Title { get; set; }
        [BsonRequired] public string Content { get; set; }
    }
}
