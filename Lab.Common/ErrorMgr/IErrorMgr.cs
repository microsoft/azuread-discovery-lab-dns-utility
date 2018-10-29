using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lab.Common.Infra
{
    public interface IErrorMgr
    {
        Task<ErrorPoco> ReadError(string id, bool track=true);
        Task<bool> SaveError(ErrorPoco eo);
        Task<ErrResponsePoco> InsertError(Exception err, string message = "", string userComment="");
        Task<IEnumerable<ErrorPoco>> GetErrorItems(int count = 100);
    }
}
