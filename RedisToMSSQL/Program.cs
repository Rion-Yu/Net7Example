using Microsoft.Extensions.Hosting;
using RedisToMSSQL.DataContexts;
using Microsoft.Extensions.DependencyInjection;
using RedisToMSSQL.Service;
using RedisToMSSQL.Interface;
using Microsoft.Extensions.Configuration;
using RedisToMSSQL.Repository;

var host = CreateHostBuilder(args).Build();
host.Run();

static IHostBuilder CreateHostBuilder(string[] args)
{
    var environment = "Dev";

    return Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((context, config) =>
        {
            config.AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange:true);
        })
        .ConfigureServices(services =>
        {
            services.AddHostedService<ScheduledTaskService>();
            //services.AddHostedService<TgBotHost>();
            services.AddDbContext<DataContext>();
            services.AddScoped<IRepository, MsSqlRepository>(); // 或者使用 OracleRepository
            services.AddScoped<IDataService, DataService>();
        });
}

