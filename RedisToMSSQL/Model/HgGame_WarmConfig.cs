using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedisToMSSQL.Model
{
    class HgGame_WarmConfig
    {
        public int LID { get; set; }
        public short WarmType { get; set; }
        public string WarmDesc { get; set; }
        public int GameID { get; set; }
        public string GameName { get; set; }
        public float GroupID { get; set; }
        public string GroupName { get; set; }
        public long WarmCount { get; set; }
        public string MailBox { get; set; }
        public float WarnLevel { get; set; }
        public string Mark { get; set; }
        public string WarnDingPhone { get; set; }
    }
}
