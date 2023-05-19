using FastExpressionCompiler.LightExpression;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSOInventory.Data
{
    internal abstract class RepositoryBase<T> : IRepository<T>
    {
        protected readonly LiteDatabase _database;

        public RepositoryBase(LiteDatabase database)
        {
            _database = database;
        }

        public void Add(T item)
        {
            _database.GetCollection<T>().Insert(item);
        }

        public void Delete(int id)
        {
            _database.GetCollection<T>().Delete(id);
        }

        public IEnumerable<T> GetAll()
        {
            var collection = _database.GetCollection<T>();
            return collection.FindAll();
        }

        public T GetById(int id)
        {
            var collection = _database.GetCollection<T>();
            return collection.FindById(id);
        }

        

        public void Update(T item)
        {
            var collection = _database.GetCollection<T>();
            collection.Update(item);
        }
    }
}
