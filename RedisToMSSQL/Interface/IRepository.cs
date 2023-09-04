using RedisToMSSQL.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedisToMSSQL.Interface
{
    public interface IRepository
    {
        bool InsertData(Base_RedisBind data);
    }
}
