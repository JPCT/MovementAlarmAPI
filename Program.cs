using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using System;
using colegal;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace MovementAlarmAPI
{
    class Program
    {
        static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateWebHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

    }
}
