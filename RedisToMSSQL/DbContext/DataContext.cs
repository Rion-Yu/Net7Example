using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System.Data;
using System.Data.SqlClient;
using System.Data.OracleClient;

namespace RedisToMSSQL.DataContexts
{
    public class DataContext : DbContext
    {
        private readonly IConfiguration _configuration;
        private readonly string _MSSQLConnection;
        private readonly string _OracleConnection;
        private readonly string _RedisConnection;
        
        public DataContext(IConfiguration configuration)
        {
            _configuration = configuration;
            var NonEncrypt = _configuration.GetValue<bool>("ConnectionStrings:NonEncrypt");
            //_connectionString = NonEncrypt ? _configuration.GetConnectionString("SqlConnection") : SecuirtyHandler.aesDecryptBase64(_configuration.GetConnectionString("SqlConnection"), "WebApiServices"); ;
            _MSSQLConnection = _configuration.GetConnectionString("MSSQLConnection");
            _OracleConnection = _configuration.GetConnectionString("OracleConnection");
            _RedisConnection = _configuration.GetConnectionString("RedisConnection");
        }

        public IDbConnection CreateConnection()
            => new SqlConnection(_MSSQLConnection);
        public IDbConnection CreateOracleConnection()
            => new OracleConnection(_OracleConnection);
        public ConnectionMultiplexer CreateRedisConnection()
            => ConnectionMultiplexer.Connect(_RedisConnection);
        public string GetMSSQLConnection()
            => _MSSQLConnection;
        public string GetOracleConnection()
            => _OracleConnection;
        public string GetRedisConnection()
            => _RedisConnection;
        


    }
}
