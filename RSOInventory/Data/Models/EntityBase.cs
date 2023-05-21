using LiteDB;

namespace RSOInventory.Data.Models
{
    internal class EntityBase
    {
        [BsonId]
        public int Id { get; set; }
    }
}