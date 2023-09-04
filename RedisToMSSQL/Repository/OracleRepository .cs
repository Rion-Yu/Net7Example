using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RedisToMSSQL.DataContexts;
using RedisToMSSQL.Extension;
using RedisToMSSQL.Interface;
using RedisToMSSQL.Model;
using System.Diagnostics;
using System.Globalization;
using System.Transactions;
using Telegram.Bot;

namespace RedisToMSSQL.Repository
{
    public class OracleRepository : IRepository
    {
        private readonly DataContext _context;

        public OracleRepository(DataContext context)
        {
            _context = context;
        }

        public bool InsertData(Base_RedisBind data)
        {
            using (var connection = _context.CreateOracleConnection())
            {
                connection.Open();
                connection.ChangeDatabase("OracleDatabase");
                // 寫入 Oracle 相關操作
            }
            return true;
        }

        // 實現其他方法...
    }
}
