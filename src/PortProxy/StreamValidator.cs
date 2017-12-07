namespace PortProxy
{
	using System;
	using System.Net.Sockets;
	using System.Threading.Tasks;

	using NLog;

	class StreamValidator : IStreamValidator
	{
		public async Task<bool> Validate(NetworkStream stream, int key)
		{
			throw new NotImplementedException();
		}

		public byte[] GenerateValiationData(int key)
		{
			throw new NotImplementedException();
		}
	}
}
