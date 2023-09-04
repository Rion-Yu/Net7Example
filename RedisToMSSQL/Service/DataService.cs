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

namespace RedisToMSSQL.Service
{
    public class DataService : IDataService
    {
        private readonly IConfiguration _configuration;
        private readonly DataContext _context;
        private readonly IRepository _repository;
        private readonly ILogger<DataService> _logger;
        private TimeZoneInfo _timeZone;

        public DataService(IConfiguration config, ILogger<DataService> logger, DataContext context, IRepository repository)
        {
            _logger = logger;
            _configuration = config;
            _context = context;
            _repository = repository;

            _logger.LogInformation($"Env:{_configuration.GetSection("Env").Get<string>()}");
            _timeZone = TimeZoneInfo.FindSystemTimeZoneById(_configuration.GetSection("TimeZone").Get<string>() ?? "Asia/Taipei");
            var OriCulture = Thread.CurrentThread.CurrentCulture;
            _logger.LogInformation($"OriCulture:{OriCulture}");
            Thread.CurrentThread.CurrentCulture = new CultureInfo(_configuration.GetSection("Language").Get<string>() ?? "en-us");
            var ChangeCulture = Thread.CurrentThread.CurrentCulture;
            _logger.LogInformation($"ChangeCulture:{ChangeCulture}");
        }

        public bool HTelBinding()
        {
            bool Result = true;
            // 指定時區            
            DateTimeOffset dateTimeOffset = DateTimeOffset.Now.ToOffset(_timeZone.GetUtcOffset(DateTimeOffset.Now));
            _logger.LogInformation($"HTelBinding sync Start at : {dateTimeOffset.ToString("yyyy-MM-dd HH:mm:ss")}");

            try
            {
                var lstBind = new List<Base_RedisBind>();
                using (var redis = _context.CreateRedisConnection())
                {
                    var rediskeys = redis.GetServer(_context.GetRedisConnection()).Keys(pattern: "HTelBinding:*");
                    var redisdb = redis.GetDatabase();
                    foreach (var key in rediskeys)
                    {
                        foreach (var subkey in redisdb.HashGetAll(key))
                        {
                            if (subkey.Value.ToString().Substring(0, 10) == dateTimeOffset.AddDays(-1).ToString("MM/dd/yyyy"))
                            {
                                lstBind.Add(new Base_RedisBind()
                                {
                                    BindValue = key.ToString().Split(':')[1],
                                    PlayerID = (int)subkey.Name,
                                    BindTime = (string)subkey.Value
                                });
                            }
                        }
                    }
                }

                using (var connection = _context.CreateConnection())
                {
                    connection.Open();
                    var insertScript = CommonFunction.GenerateInserScript(new Base_RedisBind());

                    connection.ChangeDatabase("HallGameBase");
                    connection.Execute(insertScript, lstBind);
                }
                _logger.LogInformation($"HTelBinding sync Success at : {dateTimeOffset.ToString("yyyy-MM-dd HH:mm:ss")}");
                return Result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Result;
            }
        }
        public bool HProp()
        {
            bool Result = true;
            _logger.LogInformation($"HPropCulture:{Thread.CurrentThread.CurrentCulture}");
            //參照原本預存中的道具ID
            List<string> configIDs = new List<string>() {"1","2","3","7","13","22","23","25","27","29","37","43"};

            // 指定時區            
            DateTimeOffset dateTimeOffset = DateTimeOffset.Now.ToOffset(_timeZone.GetUtcOffset(DateTimeOffset.Now));

            DateTimeOffset yesterday = dateTimeOffset.AddDays(-1);
            DateTimeOffset previousDayStart = new DateTimeOffset(yesterday.Year, yesterday.Month, yesterday.Day, 0, 0, 0, yesterday.Offset);
            _logger.LogInformation($"PreviousDayStart:{previousDayStart}");

            _logger.LogDebug($"RedisConnectString:{_context.GetRedisConnection()}");
            _logger.LogDebug($"MSSQL:MSSQLConnectString:{_context.GetMSSQLConnection()}");

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            _logger.LogInformation($"HProp sync Start at : {dateTimeOffset.ToString("yyyy-MM-dd HH:mm:ss")}");
            try
            {
                var Props = new List<Base_RedisProp>();
                string timestamp = previousDayStart.ToString(@"yyyy\-MM\-dd HH\:mm\:ss\.fff");
                string format = @"yyyy\-MM\-dd HH\:mm\:ss\.fff";
                DateTime createTime = DateTime.ParseExact(timestamp, format, Thread.CurrentThread.CurrentCulture);

                _logger.LogInformation($"Redis:CreateRedisConnection");
                using (var redis = _context.CreateRedisConnection())
                {
                    _logger.LogInformation($"Redis:GetServer");
                    var rediskeys = redis.GetServer(_context.GetRedisConnection()).Keys(pattern: "HProp:*");
                    var redisdb = redis.GetDatabase();

                    //過濾髒資料(正確key範例：HProp:10065280)
                    var filteredKeys = rediskeys.Where(key => key.ToString().Length == 14);

                    foreach (var key in filteredKeys)
                    {
                        _logger.LogDebug($"PlayerID: {Convert.ToInt32(key.ToString().Split(':')[1])}.");

                        foreach (var subkey in redisdb.HashGetAll(key).Where(c => configIDs.Contains(c.Name)))
                        {
                            _logger.LogDebug($"ConfigId:{subkey.Name}.");

                            Props.Add(new Base_RedisProp()
                            {
                                PlayerID = Convert.ToInt32(key.ToString().Split(':')[1]),
                                ConfigId = Convert.ToInt32(subkey.Name),
                                Count = Convert.ToInt64(subkey.Value),
                                CreateTime = createTime
                            });
                        }
                    }
                }

                _logger.LogInformation($"HProp Count:{Props.Count}");
                _logger.LogInformation($"HProp CreateTime:{Props.FirstOrDefault()?.CreateTime}");
                using (var ts = new TransactionScope())
                {
                    using (var connection = _context.CreateConnection())
                    {
                        _logger.LogInformation($"MSSQL:CreateConnection:Success");
                        connection.Open();
                        _logger.LogInformation($"MSSQL:Open:Success");
                        
                        var insertScript = CommonFunction.GenerateInserScript(new Base_RedisProp());
                        //無法正常寫入時，改為下列語法強制再轉一次DateTime2
                        //var insertScript = $"INSERT INTO Base_RedisProp VALUES(@PlayerID,@ConfigId,@Count,CONVERT(datetime2,@CreateTime))";
                        _logger.LogInformation($"insertScript:{insertScript}");
                        
                        connection.ChangeDatabase("HallGameBase");
                        connection.Execute(insertScript, Props);
                    }
                    ts.Complete();
                }
                _logger.LogInformation($"HProp sync Success at : {dateTimeOffset.ToString("yyyy-MM-dd HH:mm:ss")}");
                stopwatch.Stop();
                _logger.LogInformation($"執行時間：{stopwatch.ElapsedMilliseconds / 1000.0} 秒");
                return Result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                _logger.LogError($"InnerException:{ex.InnerException.Message}");
                return Result;
            }
        }
        public bool Warm()
        {
            bool Result = true;
            // 指定時區            
            DateTimeOffset dateTimeOffset = DateTimeOffset.Now.ToOffset(_timeZone.GetUtcOffset(DateTimeOffset.Now));
            _logger.LogInformation($"HTelBinding sync Start at : {dateTimeOffset.ToString("yyyy-MM-dd HH:mm:ss")}");

            try
            {
                var tg = new TelegramBotClient("6227954322:AAGxIS1DBGqqyD9S9ql3_tINIKDfrNOKaXc");


                //tg.SendTextMessageAsync(ids, "Hello World3");

                string[] props = new HgGame_WarmConfig().GetType().GetProperties().Where(o => o.Name != "Password").Select(o => o.Name).ToArray();

                string Column = string.Join(",", props);

                var query = $"SELECT " + Column + " FROM HgGame_WarmConfig Where WarmType = 3 ";

                using (var connection = _context.CreateConnection())
                {
                    connection.Open();
                    connection.ChangeDatabase("HallGameMS_V2");
                    var HgGame_WarmConfigs = connection.Query<HgGame_WarmConfig>(query);

                    foreach(var warm in HgGame_WarmConfigs) 
                    {
                        switch (warm.WarmType) 
                        {
                            case 3://線上人數預警
                                var ids = warm.WarnDingPhone.ToString().Split(';').ToList();
                                ids.ForEach(id => tg.SendTextMessageAsync(id, "每五分钟各游戏在线预警"));
                                break;
                        }
                    }                   
                }
                _logger.LogInformation($"HTelBinding sync Success at : {dateTimeOffset.ToString("yyyy-MM-dd HH:mm:ss")}");
                return Result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Result;
            }
        }
    }
}
