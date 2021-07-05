using MongoDB.Bson.Serialization.Attributes;

namespace FetchApiTutorial.Models
{
    public class MyTask
    {
        [BsonId, BsonRequired] public string Id { get; set; }
        [BsonRequired] public string Title { get; set; }
        [BsonRequired] public string Content { get; set; }
    }
}
