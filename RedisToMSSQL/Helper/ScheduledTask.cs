using Microsoft.Extensions.Logging;
using RedisToMSSQL.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RedisToMSSQL.Helper
{
    public class ScheduledTask : IScheduledTask
    {
        private readonly ILogger<ScheduledTask> _logger;
        private readonly IDataService _dataService;
        private readonly string _name;
        private readonly DateTime _jobTime;
        private readonly TimeSpan _interval;
        private readonly Timer _timer;

        public ScheduledTask(IDataService dataService, string name,DateTime jobTime)
        {
            _name = name;
            _jobTime = jobTime;
            _dataService = dataService;
        }
        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            switch (_name) 
            {
                case "HTelBinding":
                    _dataService.HTelBinding();
                    break;
                case "HProp":
                    _dataService.HProp();
                    break;
                case "Warm":
                    _dataService.Warm();
                    break;
            }
        }
        public DateTime JobTime => _jobTime;
        public string JobName => _name;

        //public void Start()
        //{
        //    _timer.Change(TimeSpan.Zero, _interval);
        //}

        //public void Stop()
        //{
        //    _timer.Change(Timeout.Infinite, Timeout.Infinite);
        //}
    }

}
