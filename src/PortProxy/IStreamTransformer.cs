namespace PortProxy
{
	/// <summary>
	/// 流变换类
	/// </summary>
	public interface IStreamTransformer
	{
		/// <summary>
		/// 对指定 <paramref name="buffer"/> 里指定偏移 <paramref name="offset"/> 处的 <paramref name="length"/> 个字节进行原位变换（混淆）
		/// </summary>
		/// <param name="buffer">缓冲数组</param>
		/// <param name="offset">偏移</param>
		/// <param name="length">长度</param>
		void Encode(byte[] buffer, int offset, int length);

		/// <summary>
		/// 对指定 <paramref name="buffer"/> 里指定偏移 <paramref name="offset"/> 处的 <paramref name="length"/> 个字节进行原位变换（还原）
		/// </summary>
		/// <param name="buffer">缓冲数组</param>
		/// <param name="offset">偏移</param>
		/// <param name="length">长度</param>
		void Decode(byte[] buffer, int offset, int length);

		/// <summary>
		/// 初始化
		/// </summary>
		/// <param name="keySource">初始化的KEY，通常是服务端端口</param>
		/// <param name="keyIndex">初始化使用的KEY索引，允许针对一个KEY生成多个混淆方式，同一个连接的上传下载两个通道使用不同的混淆方式，可能取值为0,1</param>
		void Init(int keySource, int keyIndex);
	}
}
