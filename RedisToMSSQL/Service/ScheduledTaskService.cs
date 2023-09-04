using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RedisToMSSQL.Helper;
using RedisToMSSQL.Interface;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RedisToMSSQL.Service
{
    public class ScheduledTaskService : IHostedService, IDisposable
    {
        private readonly List<IScheduledTask> _tasks = new List<IScheduledTask>();
        private readonly ILogger<ScheduledTaskService> _logger;
        private readonly IConfiguration _configuration;
        private TimeZoneInfo _timeZone;
        private Timer _timer;
        private IDataService _service;
        private CancellationTokenSource _cancellationTokenSource;
        private record Job(string JobName,string JobTime);
        public ScheduledTaskService(ILogger<ScheduledTaskService> logger,IDataService service,IConfiguration configuration)
        {
            _logger = logger;
            _service = service;
            _configuration = configuration;
            _cancellationTokenSource = new CancellationTokenSource();
            _timeZone = TimeZoneInfo.FindSystemTimeZoneById(_configuration.GetSection("TimeZone").Get<string>() ?? "Asia/Taipei");
            Init();
        }
        private void Init()
        {
            var jobs = _configuration.GetSection("Jobs").Get<List<Job>>();
            
            foreach (var job in jobs)
            {
                DateTime.TryParse(job.JobTime, out DateTime jd);
                AddTask(new ScheduledTask(_service,job.JobName, jd));
                _logger.LogInformation($"AddTask {job.JobName} {job.JobTime}.");
            }
        }
        public void AddTask(IScheduledTask task)
        {
            _tasks.Add(task);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("RedisToMSSQL service is starting.");
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(60));

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            // 指定時區
            DateTimeOffset dateTimeOffset = DateTimeOffset.Now.ToOffset(_timeZone.GetUtcOffset(DateTimeOffset.Now));
            _logger.LogDebug($"DoWork at {dateTimeOffset.ToString("HH:mm")}.");
            foreach (var task in _tasks)
            {
                //DateTime.ToString(@"HH\:mm") for linux
                _logger.LogDebug($"Check Job at {dateTimeOffset.ToString("HH:mm")}. JobName: {task.JobName} : JobTime: {task.JobTime.ToString(@"HH\:mm")}.");
                if (task.JobTime.ToString(@"HH\:mm") == dateTimeOffset.ToString("HH:mm"))
                {
                    _logger.LogInformation($"Do {task.JobName} at {task.JobTime.ToString(@"HH\:mm")}.");
                    task.ExecuteAsync(_cancellationTokenSource.Token);
                }
            }            
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("RedisToMSSQL service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}

