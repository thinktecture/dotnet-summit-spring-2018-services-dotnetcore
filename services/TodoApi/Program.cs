using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace TodoApi
{
	public class Program
	{
		public static void Main(string[] args)
		{
			using (var host = BuildWebHost(args))
			{
				var configuration = host.Services.GetService<IConfiguration>();
				var instrumentationKey = configuration.GetSection("ApplicationInsights").GetValue<string>("InstrumentationKey");

				var loggerConfig = new LoggerConfiguration()
					.Enrich.FromLogContext()
					.Enrich.WithThreadId()
					.Enrich.WithProcessId()
					.Enrich.WithProcessName()
					.MinimumLevel.Debug()
					.ReadFrom.Configuration(configuration)
					.WriteTo.Console();

				if (!String.IsNullOrEmpty(instrumentationKey))
				{
					loggerConfig = loggerConfig
						.WriteTo.ApplicationInsightsEvents(instrumentationKey);
				}

				Log.Logger = loggerConfig.CreateLogger();

				try
				{
					host.Run();
				}
				finally
				{
					Log.CloseAndFlush();
				}
			}
		}

		public static IWebHost BuildWebHost(string[] args) =>
			WebHost.CreateDefaultBuilder(args)
				.UseStartup<Startup>()
				.UseSerilog()
				.UseApplicationInsights()
				.Build();
	}
}
