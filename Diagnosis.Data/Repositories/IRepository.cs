using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.Data.Repositories
{
    public interface IRepository<T> where T : class
    {
        void Add(T entity);
        void Update(T entity);
        void Remove(T entity);
        IEnumerable<T> GetAll();
        T GetById(int entityId);
        T GetByName(string name);
    }
}
