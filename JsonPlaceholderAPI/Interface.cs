using System.Collections.Generic;
using System.Threading.Tasks;

namespace JsonPlaceholderAPI.Repositories
{
    // Generic IRepository arayüzü
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();       // Tüm nesneleri al
        Task<T> GetByIdAsync(int id);             // Id'ye göre bir nesne al
        Task AddAsync(T entity);                  // Yeni bir nesne ekle
        void Update(T entity);                    // Var olan bir nesneyi güncelle
        Task DeleteAsync(int id);                 // Id'ye göre bir nesneyi sil
    }
}
