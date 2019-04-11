using DocDBLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Lab.Common.Repo
{
    public class BaseRepo<TObject> : IRepo<TObject> where TObject : class, IDocModelBase
    {
        public virtual async Task<IEnumerable<TObject>> GetAllAsync()
        {
            return await DocDBRepo.DB<TObject>.GetItemsAsync();
        }

        public virtual async Task<IEnumerable<TObject>> GetItemsAsync(Expression<Func<TObject, bool>> predicate)
        {
            return await DocDBRepo.DB<TObject>.GetItemsAsync(predicate);
        }
        public virtual async Task<IEnumerable<TObject>> GetItemsAsync()
        {
            return await DocDBRepo.DB<TObject>.GetItemsAsync();
        }

        public virtual async Task<TObject> GetAsync(string id)
        {
            return await ((id == null) ? null : DocDBRepo.DB<TObject>.GetItemAsync(id));
        }

        public virtual async Task<TObject> Upsert(TObject item)
        {
            if (item.Id == null)
            {
                item = await DocDBRepo.DB<TObject>.CreateItemAsync(item);
            }
            else
            {
                item = await DocDBRepo.DB<TObject>.UpdateItemAsync(item);
            }
            return item;
        }

        public virtual async Task<int> Delete(string id)
        {
            TObject item = await GetAsync(id);
            await DocDBRepo.DB<TObject>.DeleteItemAsync(item);
            return 1;
        }
    }
}
