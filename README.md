## PortProxy

> 不能多言，言多必死，死后鞭尸。

一个简单的端口转发工具，只不过中间允许你加入自己的流验证和流变换算法。

## 运行平台

基于 .NET CORE 2.0 编写，支持Windows/Linux/Mac等平台。  
分为本地端和远程端，将远程端上指定地址的端口（如远程端上的Socks5服务器端口）映射到本地。  
所以请发挥自己的想象去创造用途，不能多言，言多必死，死后鞭尸。

## 开发配置

- [.NET CORE 2.0](https://www.microsoft.com/net/download/windows)
- [Jetbrains Rider](https://www.jetbrains.com/rider/) Or [VS2017 Community](https://www.visualstudio.com/thank-you-downloading-visual-studio/?sku=community&rel=15)

## 注意

** 这个代码是不完整的！`StreamValidator` 和 `StreamTransformer` 没有给出任何默认实现，请依据文档发挥自己的创意写出相关的代码。**

** 本地生成验证数据、服务端校验认证数据、流变换双向都是要进行的操作。**

## 附言

似乎用在Linux上（比如CentOS7）要比Windows上更简单诶。

比如服务端配置在远程CentOS7上：

1. 安装.net core(假定安装在`/usr/bin/dotnet`)
2. 编译后的程序发布到 `/data/server/portproxy/`
3. 将仓库中的 `portproxy.service` 文件复制到`/lib/systemd/system/`，如果路径和上述的不一致记得先改下
4. 执行 `systemctl start portproxy` 和 `systemctl enable portproxy` 启用

默认本地端口和远程端口都是`10240`，支持参数为：

- **--local** 指定工作模式为本地模式，否则为远程模式
- **--port=&lt;int&gt;** 当前监听端口，默认本地模式为`1080`，远程模式为`10240`
- **--buffer=&lt;int&gt;** 用于流操作的缓冲区大小，单位为`KB`，默认为`128KB`
- **--server=&lt;ADDRESS&gt;** 指定上游服务器地址，对于本地模式是指远程对应的服务器，对于远程模式则指的是转发的目标服务器
- **--serverport=&lt;int&gt;** 指定上游服务器端口，对于本地模式默认为`10240`，对于远程模式默认为`1080`

如果需要修改默认参数，记得修改上述的service文件。

对于Windows机器，貌似可以直接用 `dotnet PortProxy.dll &lt;参数&gt;`直接运行，至于服务化嘛……懒得搞……完全可以搞成一个Windows Service，有兴趣的自己搞了。
