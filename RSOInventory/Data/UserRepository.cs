using LiteDB;
using RSOInventory.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSOInventory.Data
{
    internal class UserRepository : RepositoryBase<User>, IUserRepository
    {
        public UserRepository(LiteDatabase database) : base(database)
        {
        }
    }
}
