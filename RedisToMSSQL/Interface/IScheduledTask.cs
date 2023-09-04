using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedisToMSSQL.Interface
{
    public interface IScheduledTask
    {
        string JobName { get; }
        DateTime JobTime { get; }
        Task ExecuteAsync(CancellationToken cancellationToken);

    }

}
