using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace UdemySiparis.Data.Repository.IRepository
{
    public interface IRepository<T>where T : class    // T gelecek modeli temsil ediyor , yani category, product...
    {
        void Add(T entity);
        T GetFirstOrDefault(Expression<Func<T,bool>> filter,                 // zorunlu tutmamız gerektigi tanımları burda yazacagız 
            string? includeProperties = null); 
        IEnumerable<T> GetAll(Expression<Func<T,bool>>? filter= null,              // altındaki için:butun kategorini listesini alabiliriz, yada ahmet isimli kullanıcıya ait sipariş listeyebiliriz...
            string? includeProperties = null);                             // aynısını koplayadık cunku sorgulama tipi aynı, fark sadece burda liste sorgulıyoruz 
        void Update(T entity);                                              // bu tercih, bilgi guncellemesini istemezsen kullanma
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);
    }

}
