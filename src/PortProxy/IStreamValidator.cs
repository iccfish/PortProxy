namespace PortProxy
{
	using System.Net.Sockets;
	using System.Threading.Tasks;

	/// <summary>
	/// 流正确性认证提供类
	/// </summary>
	public interface IStreamValidator
	{
		/// <summary>
		/// 确认指定的流是否符合规则，仅服务端认证
		/// </summary>
		/// <param name="stream">来源网络流</param>
		/// <param name="key">认证的KEY，通常是端口</param>
		/// <returns>认证成功则返回 <see langword="true">true</see></returns>
		Task<bool> Validate(NetworkStream stream, int key);

		/// <summary>
		/// 生成验证数据以便于提交服务端认证（仅本地）
		/// </summary>
		/// <param name="key">认证的KEY，通常是端口</param>
		/// <returns>要发送给远程服务器提交认证的数据</returns>
		byte[] GenerateValiationData(int key);
	}
}
