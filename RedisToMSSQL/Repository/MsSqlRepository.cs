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
    public class MsSqlRepository : IRepository
    {
        private readonly DataContext _context;

        public MsSqlRepository(DataContext context)
        {
            _context = context;
        }

        public bool InsertData(Base_RedisBind data)
        {
            using (var connection = _context.CreateConnection())
            {
                connection.Open();
                connection.ChangeDatabase("HallGameBase");
                // 寫入 MSSQL 相關操作
            }
            return true;
        }

        // 實現其他方法...
    }
}
