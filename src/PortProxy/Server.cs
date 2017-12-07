namespace PortProxy
{
	using System;
	using System.Diagnostics;
	using System.Net;
	using System.Net.Sockets;
	using System.Runtime.InteropServices;
	using System.Threading.Tasks;

	using Microsoft.Extensions.DependencyInjection;
	using Microsoft.Extensions.Logging;

	using NLog;

	public class Server
	{
		private TcpListener _listener;
		private ILogger<Server> _logger;
		private int _bufferSize, _port;
		private bool _local;
		private IServiceProvider _serviceProvider;

		public string RemoteServer { get; private set; }
		public int RemoteServerPort { get; private set; }

		public Server(ILogger<Server> logger, IServiceProvider serviceProvider)
		{
			_logger = logger;
			_serviceProvider = serviceProvider;

			Debug.Assert(_serviceProvider != null, "No IServiceProvider found.");
		}

		public void Init(int bufferSize, int port, bool local, string remoteServer, int remoteServerPort)
		{
			_bufferSize = bufferSize;
			_port = port;
			_local = local;
			RemoteServer = remoteServer;
			RemoteServerPort = remoteServerPort;
		}

		/// <summary>
		/// 获得或设置是否是本地模式
		/// </summary>
		public bool Local => _local;

		public async void Start()
		{
			_logger.LogInformation("正在启动服务器端监听...");
			_listener = new TcpListener(IPAddress.Any, _port);
			_listener.Start();
			_logger.LogInformation($"服务器监听在端口 {_port}, 本地模式 {_local}...");

			var connectionCount = 0L;
			_logger.LogInformation("等待客户端连接...");

			while (true)
			{
				var client = await _listener.AcceptTcpClientAsync();
				connectionCount++;

				_logger.LogInformation($"#{connectionCount} 新的客户端连接 {client.Client.RemoteEndPoint} -> {client.Client.LocalEndPoint}");

				ProcessClientAsync(connectionCount, client);
			}
		}

		async void ProcessClientAsync(long connectionCount, TcpClient client)
		{
			IStreamTransformer CreateStreamTransformer(int key, int keyIndex)
			{
				var t = _serviceProvider.GetRequiredService<IStreamTransformer>();
				t.Init(key, keyIndex);
				return t;
			}

			var stream = client.GetStream();

			_logger.LogInformation($"#{connectionCount} 正在验证");
			var port = _local ? RemoteServerPort : _port;
			//验证
			var valid = false;
			var validator = _serviceProvider.GetRequiredService<IStreamValidator>();
			if (!Local)
			{
				try
				{
					_logger.LogInformation($"#{connectionCount} 正在验证请求");
					valid = await validator.Validate(stream, port);
				}
				catch (Exception e)
				{
					_logger.LogInformation($"#{connectionCount} 验证错误：{e.Message}");
				}
				finally
				{
					_logger.LogInformation($"#{connectionCount} 验证结果：{valid}");
					if (!valid)
					{
						client.Close();
					}
				}
			}
			else
			{
				valid = true;
			}

			//parent
			var upclient = new TcpClient();
			NetworkStream upstream = null;
			if (valid)
			{
				try
				{
					_logger.LogInformation($"#{connectionCount} 正在连接上游服务器");
					await upclient.ConnectAsync(RemoteServer, RemoteServerPort);
					upstream = upclient.GetStream();
					_logger.LogInformation($"#{connectionCount} 上游服务器连接已打开 {upclient.Client.LocalEndPoint} -> {upclient.Client.RemoteEndPoint}");
					if (_local)
					{
						var buffer = validator.GenerateValiationData(port);
						await upstream.WriteAsync(buffer, 0, buffer.Length);
					}
				}
				catch (Exception e)
				{
					_logger.LogError($"#{connectionCount} 未能为打开上游服务器连接: {e.Message}");
					client.Close();
					valid = false;
				}

				if (valid)
				{
					await Task.WhenAny(
						ProcessStreamCopyAsync(stream, upstream, _local ? null : CreateStreamTransformer(port, 1), _local ? CreateStreamTransformer(port, 1) : null),
						ProcessStreamCopyAsync(upstream, stream, _local ? CreateStreamTransformer(port, 0) : null, _local ? null : CreateStreamTransformer(port, 0))
					);
				}
			}

			try
			{
				upstream?.Dispose();
				stream.Dispose();
				upclient.Client.Close();
				client.Client.Close();
				upclient.Close();
				client.Close();
			}
			catch (Exception e)
			{
				_logger.LogError($"#{connectionCount} 尝试关闭连接的时候发生错误 {e.Message}");
			}

			_logger.LogInformation($"#{connectionCount} 连接已关闭");
		}

		bool IsSocketConnected(Socket socket)
		{
			if (!socket.Connected)
				return false;

			return !(socket.Poll(0, SelectMode.SelectRead) && socket.Available == 0);
		}

		async Task ProcessStreamCopyAsync(NetworkStream srcStream, NetworkStream dstStream, IStreamTransformer readTransformer, IStreamTransformer writeTransformer)
		{
			var count = 0;
			var buffer = new byte[_bufferSize];
			do
			{
				try
				{
					count = await srcStream.ReadAsync(buffer, 0, buffer.Length);
					if (count == 0)
						continue;

					readTransformer?.Decode(buffer, 0, count);
					writeTransformer?.Encode(buffer, 0, count);
					await dstStream.WriteAsync(buffer, 0, count);
				}
				catch (Exception e)
				{
					break;
				}
			} while (count > 0);
		}
	}
}
