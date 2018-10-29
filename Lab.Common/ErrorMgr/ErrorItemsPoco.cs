using System.Collections.Generic;

namespace Lab.Common.Infra
{
    public class ErrorItemsPoco
    {
        public int RecordCount { get; set; }
        public IEnumerable<ErrorPoco> ErrorItems { get; set; }
    }
}
