using System;

namespace PortProxy
{
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Reflection;
	using System.Threading;

	using Microsoft.Extensions.DependencyInjection;
	using Microsoft.Extensions.Logging;

	using NLog;
	using NLog.Extensions.Logging;

	class Program
	{
		static void Main(string[] args)
		{
			//# corefx bug of https://github.com/dotnet/corefx/issues/24832
			new ArgumentException();

			ConfigTraditionalLog();
			var serviceProvider = BuildDi();
			var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

			//log

			var local = args.Any(s => s == "--local");
			var port = GetOptionValue(args.FirstOrDefault(s => s.StartsWith("--port="))).ToInt32(local ? 1080 : 10240);
			var buffer = GetOptionValue(args.FirstOrDefault(s => s.StartsWith("--buffer="))).ToInt32(128) * 1024;
			var server = GetOptionValue(args.FirstOrDefault(s => s.StartsWith("--server="))).DefaultForEmpty(local ? "" : "127.0.0.1");
			var serverport = GetOptionValue(args.FirstOrDefault(s => s.StartsWith("--serverport="))).ToInt32(!local ? 1080 : 10240);

			if (local && server.IsNullOrEmpty())
			{
				logger.LogError("请指定一个远程服务器地址");
				return;
			}

			var srv = serviceProvider.GetRequiredService<Server>();
			srv.Init(buffer, port, local, server, serverport);
			srv.Start();

			while (true)
			{
				Thread.Sleep(100);
			}
		}

		static string GetOptionValue(string str)
		{
			if (string.IsNullOrEmpty(str))
				return null;
			return str.Substring(str.IndexOf('=') + 1);
		}

		static void ConfigTraditionalLog()
		{
			//workaround for log callsite bug (see https://github.com/NLog/NLog.Extensions.Logging/issues/165)
			LogManager.AddHiddenAssembly(Assembly.Load(new AssemblyName("Microsoft.Extensions.Logging")));
			LogManager.AddHiddenAssembly(Assembly.Load(new AssemblyName("Microsoft.Extensions.Logging.Abstractions")));
			LogManager.AddHiddenAssembly(Assembly.Load(new AssemblyName("NLog.Extensions.Logging")));
		}

		private static IServiceProvider BuildDi()
		{
			var services = new ServiceCollection();

			services.AddTransient<Server>();
			services.AddTransient<IStreamValidator, StreamValidator>();
			services.AddTransient<IStreamTransformer, StreamTransformer>();

			services.AddSingleton<ILoggerFactory, LoggerFactory>();
			services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));

			var serviceProvider = services.BuildServiceProvider();

			var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
			loggerFactory.AddNLog(new NLogProviderOptions {CaptureMessageTemplates = true, CaptureMessageProperties = true});
			loggerFactory.ConfigureNLog("nlog.config");

			return serviceProvider;
		}
	}
}
