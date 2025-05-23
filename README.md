# ChattingRoom 聊天室系统

> 基于 C# 和 .NET Framework 开发的桌面端 TCP 聊天服务器与客户端系统。


##  项目概述

ChattingRoom 是一个功能完备的局域网聊天室项目，采用 C# 编写，基于 TCP 通信协议，支持注册、登录、群聊、私聊、好友系统、头像上传、聊天记录存储等功能，结合 MySQL 数据库实现用户与消息持久化，并提供友好的 Windows Forms 图形界面。

---


##  技术栈

- **编程语言**：C#
- **界面框架**：Windows Forms
- **数据库**：MySQL
- **协议**：基于 TCP 的自定义消息协议
- **依赖**：
  - [MySql.Data](https://www.nuget.org/packages/MySql.Data/)
  - System.Net.Sockets, System.Threading, System.Xml 等

---

## 功能说明

###  注册 / 登录

- 用户通过唯一 UID 登录系统
- UID 在注册时自动生成并写入数据库
- 本地可记住登录信息
- 登录成功后自动加载好友与头像

### 群聊机制

- 群聊 UID 固定为 `"000000"`
- 所有客户端接收该 UID 的消息
- 消息中发送者会被替换为用户名（若为好友）

###  私聊机制

- 双方 UID 明确，仅接收与自己相关的消息
- 支持文本与文件消息
- 非当前聊天对象发消息时触发红点提醒

###  好友系统

- 新用户初始无好友，仅可群聊
- 用户可输入 UID 搜索添加好友
- 好友数据持久化，支持私聊与用户名显示

###  头像支持

- 用户可设置并上传头像（存入数据库）
- 若无头像则显示默认图标（来自 Resources.resx）
- 好友列表与聊天面板均显示头像

###  聊天记录

- 消息存储于数据库表 `chatinfo`
- 支持历史记录加载
- 文件消息 Base64 解码后自动保存本地

---

##  项目架构简述

- **客户端**：
  - Windows Forms 实现多面板界面
  - 支持 UID 登录、聊天、好友操作
  - 通过 TcpClient 与服务器通信

- **服务器端**：
  - 使用 TcpListener 异步监听客户端连接
  - 支持多用户并发处理
  - 使用 CancellationToken 管理服务终止
  - 所有数据交互通过 MySQL 实现持久化

---

##  运行方法

### 1. 环境准备

- 安装 Visual Studio 2019 或以上
- 安装 .NET Framework 4.7.2
- 配置 MySQL 连接参数 
  
修改WeChattingServer/WeChattingServer/server_config.xml以及WeChattingClient/WeChattingClient/client_config.xml

示例：
```
<ClientConfig>
	<UID></UID>
	<Password></Password>
	<Database>
		<Host>localhost</Host>
		<Port>3306</Port>
		<Name>wechatting</Name>
		<User>root</User>//修改为实际mysql用户
		<DbPassword>123456</DbPassword>//修改为实际密码
	</Database>
</ClientConfig>
```
### 2. 编译运行

####  启动服务端
1. 打开 `WeChattingServer.sln`
2. 编译并运行，项目会自动创建数据库、建表
3. 点击开始监听，实现服务端监听客户端连接

####  启动客户端
1. 打开 `WeChattingClient.sln`
2. 编译并运行客户端，连接到服务端 IP 和端口
3. 注册新用户或登录已有账号开始聊天

---

