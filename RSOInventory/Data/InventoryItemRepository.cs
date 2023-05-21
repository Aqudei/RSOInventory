using LiteDB;
using RSOInventory.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSOInventory.Data
{
    internal class InventoryItemRepository : RepositoryBase<InventoryItem>, IInventoryItemRepository
    {
        public InventoryItemRepository(LiteDatabase database) : base(database) { }

        public IEnumerable<InventoryItem> GetParentItems()
        {
            var collection = _database.GetCollection<InventoryItem>().Include(i=>i.EndUser);
            return collection.Find(i => i.ParentId == 0);
        }

        public IEnumerable<InventoryItem> ListChildren(int parentId)
        {
            var collection = _database.GetCollection<InventoryItem>();
            return collection.Find(i => i.ParentId == parentId);
        }
    }
}
