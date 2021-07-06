using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace FetchApiTutorial.Models
{
    public class MyTask
    {
        [BsonId(IdGenerator = typeof(ObjectIdGenerator)), BsonRepresentation(BsonType.ObjectId), BsonRequired] public ObjectId Id { get; set; }
        [BsonRequired] public string Title { get; set; }
        [BsonRequired] public string Content { get; set; }
    }
}
