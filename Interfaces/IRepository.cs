using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccess.MsSql.DapperRepository.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<T> Get(long key);

        Task<T> Get(int key);

        Task<IEnumerable<T>> GetAll();

        Task<IEnumerable<T>> GetDataWithQuery(string query, object parameters);

        Task<IEnumerable<T>> GetDataWithProc(string proc, object parameters);

        Task<IEnumerable<T>> GetDataByFilter(string table = "", params KeyValuePair<string, string>[] filter);

        Task<IEnumerable<T>> GetDataByFilter(params KeyValuePair<string, string>[] filter);

        Task<IEnumerable<T>> GetDataByFilter(object filter, string table = "");

        Task ExecuteProc(string proc, object parameters);

        Task ExecuteStatement(string statement, object parameters);

        Task<long> Insert(T entity);

        Task<long> Insert(IEnumerable<T> entities);

        Task<bool> Delete(T entity);

        Task<bool> Delete(IEnumerable<T> entities);

        Task<bool> Update(T entity);

        Task<bool> Update(IEnumerable<T> entities);

    }
}

