using RSOInventory.Data;
using RSOInventory.Data.Models;

namespace RSOInventory.Events
{


    internal class CrudEvent<T> where T : EntityBase, new()
    {
        internal enum CrudActionType
        {
            Created, Updated, Deleted
        }

        public CrudActionType CrudAction { get; set; }
        public T Entity { get; set; }
    }
}