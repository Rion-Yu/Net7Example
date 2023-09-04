using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedisToMSSQL.Interface
{
    public interface IScheduledTaskService
    {
        void AddTask(IScheduledTask task);
        Task StartAsync();
        void Stop();
    }

}
