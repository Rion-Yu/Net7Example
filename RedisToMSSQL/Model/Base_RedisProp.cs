using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedisToMSSQL.Model
{
    public class Base_RedisProp
    {
        public int PlayerID { get; set; }
        public int ConfigId { get; set; }
        public long Count { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
