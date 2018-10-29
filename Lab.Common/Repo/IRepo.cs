using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab.Common.Repo
{
    interface IRepo<TObject>
    {
        Task<IEnumerable<TObject>> GetAllAsync();

        Task<TObject> GetAsync(string id);

        Task<TObject> Upsert(TObject item);

        Task<int> Delete(string id);
    }
}
