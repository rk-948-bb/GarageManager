using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GarageApi.Models
{
    public class Garage
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string? ExternalId { get; set; }
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? Telephone { get; set; }
        public string? Mikud { get; set; }
    }
}
