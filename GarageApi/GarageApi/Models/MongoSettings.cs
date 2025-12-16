namespace GarageApi.Models
{
    public class MongoSettings
    {
        public string ConnectionString { get; set; } = null!;
        public string Database { get; set; } = null!;
        public string GaragesCollection { get; set; } = null!;
    }
}
