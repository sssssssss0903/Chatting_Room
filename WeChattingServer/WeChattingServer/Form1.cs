using MySql.Data.MySqlClient;
using Mysqlx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using BCrypt.Net;

namespace WeChattingServer
{
    public partial class Form1 : Form
    {
        private int serverPort = 8888;
        private TcpListener server;
        private Thread listenThread;

        private Dictionary<string, TcpClient> uidToClient = new Dictionary<string, TcpClient>();
        private List<TcpClient> clientList = new List<TcpClient>();

        private object locker = new object();
        // 声明为空，由 LoadServerConfig() 动态设置
        private string connStr = "";
        private CancellationTokenSource cts;
        public Form1()
        {
            InitializeComponent();
            this.FormClosing += Form1_FormClosing;
        }
        private void EnsureDatabaseAndTablesExist(string host, string user, string password, string dbName, string charset)
        {
            try
            {
                // 第一步：连接到服务器，不指定 database
                string noDbConnStr = $"server={host};user id={user};password={password};charset={charset};";
                using (var conn = new MySqlConnection(noDbConnStr))
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand($"CREATE DATABASE IF NOT EXISTS `{dbName}` DEFAULT CHARSET {charset};", conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

                // 第二步：连接到目标数据库，创建表结构
                string dbConnStr = $"server={host};user id={user};password={password};database={dbName};charset={charset};";
                using (var conn = new MySqlConnection(dbConnStr))
                {
                    conn.Open();

                    string userTable = @"
    CREATE TABLE IF NOT EXISTS userinfo (
        UID VARCHAR(6) PRIMARY KEY,
        Password VARCHAR(100) NOT NULL,
        UserName VARCHAR(100) NOT NULL,
        Avatar LONGBLOB
    );";
                    new MySqlCommand(userTable, conn).ExecuteNonQuery();

                    string chatTable = @"
    CREATE TABLE IF NOT EXISTS chatinfo (
        id INT AUTO_INCREMENT PRIMARY KEY,
        sender VARCHAR(6),
        receiver VARCHAR(6),
        message TEXT,
        send_time DATETIME
    );";
                    new MySqlCommand(chatTable, conn).ExecuteNonQuery();

                    string friendTable = @"
    CREATE TABLE IF NOT EXISTS friend (
        Myaccount VARCHAR(6),
        FriendUID VARCHAR(6),
        UIDName VARCHAR(100),
        PRIMARY KEY (Myaccount, FriendUID)
    );";
                    new MySqlCommand(friendTable, conn).ExecuteNonQuery();
                }


                listServerMessage.Items.Add($"数据库 `{dbName}` 和表结构检查完成。");
            }
            catch (Exception ex)
            {
                MessageBox.Show("初始化数据库失败：" + ex.Message);
            }
        }

        private void LoadServerConfig()
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load("server_config.xml");

                // 读取端口
                string portText = doc.SelectSingleNode("/ServerConfig/Port")?.InnerText;
                if (int.TryParse(portText, out int configPort))
                {
                    serverPort = configPort;
                }

                // 读取数据库信息
                string host = doc.SelectSingleNode("/ServerConfig/Database/Host")?.InnerText;
                string user = doc.SelectSingleNode("/ServerConfig/Database/User")?.InnerText;
                string password = doc.SelectSingleNode("/ServerConfig/Database/Password")?.InnerText;
                string dbName = doc.SelectSingleNode("/ServerConfig/Database/Name")?.InnerText;
                string charset = doc.SelectSingleNode("/ServerConfig/Database/Charset")?.InnerText ?? "utf8";
                // 自动初始化数据库及表
                EnsureDatabaseAndTablesExist(host, user, password, dbName, charset);
                // 构建连接字符串
                connStr = $"server={host};user id={user};password={password};database={dbName};charset={charset};Pooling=true;MinimumPoolSize=0;MaximumPoolSize=100;";

                listServerMessage.Items.Add("已加载配置文件 server_config.xml");
            }
            catch (Exception ex)
            {
                MessageBox.Show("读取配置文件失败：" + ex.Message);
            }
        }
        private void buttonListen_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textServerPort.Text))
            {
                serverPort = Convert.ToInt32(textServerPort.Text);
            }

            server = new TcpListener(IPAddress.Any, serverPort);
            server.Start();
            listServerMessage.Items.Add($"TCP服务器启动成功，监听端口：{serverPort}");

            cts = new CancellationTokenSource(); // 初始化新的取消令牌
            ListenForClientsAsync(cts.Token);   // 启动监听，传入 token
        }

        private async void ListenForClientsAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    // 使用带取消令牌的等待方式（包裹成 Task）
                    var acceptTask = server.AcceptTcpClientAsync();
                    var completedTask = await Task.WhenAny(acceptTask, Task.Delay(-1, token));

                    if (completedTask == acceptTask)
                    {
                        TcpClient client = acceptTask.Result;

                        lock (locker)
                        {
                            clientList.Add(client);
                        }

                        _ = HandleClientCommAsync(client); // 启动客户端处理任务
                    }
                    else
                    {
                        // 取消任务触发（说明是 Task.Delay 结束）
                        break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                listServerMessage.Items.Add("服务器监听已中断（手动取消）");
            }
            catch (Exception ex)
            {
                listServerMessage.Items.Add("监听客户端异常：" + ex.Message);
            }
        }

        private async Task HandleClientCommAsync(TcpClient client)
        {
            NetworkStream stream = client.GetStream();

            try
            {
                while (true)
                {
                    byte[] lengthBuffer = new byte[4];
                    int lengthRead = await stream.ReadAsync(lengthBuffer, 0, 4);
                    if (lengthRead != 4) break;

                    int messageLength = BitConverter.ToInt32(lengthBuffer, 0);
                    if (messageLength <= 0 || messageLength > 10_000_000) break;

                    byte[] messageBuffer = new byte[messageLength];
                    int totalRead = 0;
                    while (totalRead < messageLength)
                    {
                        int read = await stream.ReadAsync(messageBuffer, totalRead, messageLength - totalRead);
                        if (read == 0) break;
                        totalRead += read;
                    }
                    if (totalRead != messageLength) break;

                    string msg = Encoding.UTF8.GetString(messageBuffer);
                    Console.WriteLine("收到消息: " + msg);
                    if (msg.StartsWith("REGISTER$"))
                    {
                        string[] parts = msg.Split('$');
                        if (parts.Length == 4)
                        {
                            string uid = parts[1];
                            string pwd = parts[2];
                            string name = parts[3];
                           
                            try
                            {
                                using (MySqlConnection conn = new MySqlConnection(connStr))
                                {
                                    await conn.OpenAsync();

                                    // 检查 UID 是否存在
                                    string checkSql = "SELECT COUNT(*) FROM userinfo WHERE UID = @uid";
                                    using (var checkCmd = new MySqlCommand(checkSql, conn))
                                    {
                                        checkCmd.Parameters.AddWithValue("@uid", uid);
                                        int count = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());

                                        if (count > 0)
                                        {
                                            await SendMessageAsync(stream, "REGISTER_FAIL$账号已存在");
                                            return;
                                        }
                                    }

                                    // 插入新用户
                                  
                                    string hashedPwd = BCrypt.Net.BCrypt.HashPassword(pwd); // 加密密码

                                    string insertSql = "INSERT INTO userinfo (UID, Password, UserName) VALUES (@uid, @pwd, @name)";
                                    using (var insertCmd = new MySqlCommand(insertSql, conn))
                                    {
                                        insertCmd.Parameters.AddWithValue("@uid", uid);
                                        insertCmd.Parameters.AddWithValue("@pwd", hashedPwd);
                                        insertCmd.Parameters.AddWithValue("@name", name);
                                        await insertCmd.ExecuteNonQueryAsync();
                                    }

                                    await SendMessageAsync(stream, "REGISTER_OK");
                                }
                            }
                            catch (Exception ex)
                            {
                                await SendMessageAsync(stream, "REGISTER_FAIL$" + ex.Message);
                            }

                            continue;
                        }
                        else
                        {
                            await SendMessageAsync(stream, "REGISTER_FAIL$格式错误");
                            continue;
                        }
                    }
                    // ==== 登录请求处理 ====
                    if (msg.StartsWith("LOGIN$"))
                    {
                        listServerMessage.Items.Add("收到登录请求：" + msg);
                        string[] parts = msg.Split('$');
                        if (parts.Length == 3)
                        {
                            string uid = parts[1];
                            string pwd = parts[2];
                            bool isOnline;
                            lock (locker)
                            {
                                isOnline = uidToClient.ContainsKey(uid);
                            }

                            if (isOnline)
                            {
                                await SendMessageAsync(stream, "LOGIN_FAIL2$该账号已在其他地方登录");
                                return;
                            }

                            try
                            {
                                using (MySqlConnection conn = new MySqlConnection(connStr))
                                {
                                    await conn.OpenAsync();
                                    string sql = "SELECT UserName, Password FROM userinfo WHERE UID = @uid";
                                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                                    {
                                        cmd.Parameters.AddWithValue("@uid", uid);
                                        cmd.Parameters.AddWithValue("@pwd", pwd);

                                        using (var reader = await cmd.ExecuteReaderAsync())
                                        {
                                            if (await reader.ReadAsync())
                                            {
                                                string userName = reader.GetString(reader.GetOrdinal("UserName"));
                                                string storedHash = reader.GetString(reader.GetOrdinal("Password"));

                                                // 验证密码
                                                if (BCrypt.Net.BCrypt.Verify(pwd, storedHash))
                                                {
                                                    string successMsg = $"LOGIN_OK${userName}";
                                                    await SendMessageAsync(stream, successMsg);
                                                }
                                                else
                                                {
                                                    await SendMessageAsync(stream, "LOGIN_FAIL");
                                                }
                                            }
                                            else
                                            {
                                                await SendMessageAsync(stream, "LOGIN_FAIL");
                                            }
                                        }

                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("登录处理失败：" + ex.Message);
                                await SendMessageAsync(stream, "LOGIN_FAIL");
                            }

                            continue; // 继续等待下一个消息
                        }
                    }

                    Match match = Regex.Match(msg, @"^(?<receiver>[#\w]+)\$(?<message>.*?)\$(?<sender>\w+)$");
                    if (!match.Success) return;

                    string receiverUID = match.Groups["receiver"].Value;
                    string messageBody = match.Groups["message"].Value;
                    string senderUID = match.Groups["sender"].Value;

                    if (!ValidateUID(senderUID) ||
                        (receiverUID != "000000" && receiverUID != "######" && !ValidateUID(receiverUID))) return;

                    if (receiverUID == "######")
                    {
                        bool isNew;
                        lock (locker)
                        {
                            isNew = !uidToClient.ContainsKey(senderUID);
                            uidToClient[senderUID] = client;
                            if (!clientList.Contains(client)) clientList.Add(client);
                        }
                        this.Invoke(new Action(() =>
                        {
                            if (isNew)
                                listServerMessage.Items.Add($"新用户上线: {senderUID}");
                            UpdateOnlineUsers();
                        }));
                        continue;
                    }

                    this.Invoke(new Action(() =>
                    {
                        listServerMessage.Items.Add($"[来自 {senderUID} 发给 {receiverUID}]：{messageBody}");
                    }));

                    string forwardMsg = messageBody + "$" + senderUID + "$" + receiverUID;
                    byte[] forwardBytes = Encoding.UTF8.GetBytes(forwardMsg);
                    byte[] forwardLength = BitConverter.GetBytes(forwardBytes.Length);
                    byte[] toSend = new byte[4 + forwardBytes.Length];
                    Buffer.BlockCopy(forwardLength, 0, toSend, 0, 4);
                    Buffer.BlockCopy(forwardBytes, 0, toSend, 4, forwardBytes.Length);

                    if (receiverUID == "000000")
                    {
                        lock (locker)
                        {
                            foreach (var other in clientList.ToList())
                            {
                                if (other.Connected)
                                {
                                    try { other.GetStream().Write(toSend, 0, toSend.Length); } catch { }
                                }
                            }
                        }
                    }
                    else
                    {
                        lock (locker)
                        {
                            if (uidToClient.TryGetValue(receiverUID, out TcpClient targetClient) && targetClient.Connected)
                            {
                                try { targetClient.GetStream().Write(toSend, 0, toSend.Length); } catch { }
                            }
                            else
                            {
                                this.Invoke(new Action(() =>
                                {
                                    listServerMessage.Items.Add($"未找到 UID={receiverUID} 的在线客户端");
                                }));
                            }
                        }
                    }

                    try
                    {
                        using (MySqlConnection conn = new MySqlConnection(connStr))
                        {
                            await conn.OpenAsync();
                            using (MySqlCommand cmd = new MySqlCommand("INSERT INTO chatinfo (sender, receiver, message, send_time) VALUES (@sender, @receiver, @message, NOW())", conn))
                            {
                                cmd.Parameters.AddWithValue("@sender", senderUID);
                                cmd.Parameters.AddWithValue("@receiver", receiverUID);
                                cmd.Parameters.AddWithValue("@message", messageBody);
                                await cmd.ExecuteNonQueryAsync();
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("保存消息失败：" + ex.Message);
                        this.Invoke(new Action(() => listServerMessage.Items.Add(ex.Message)));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("客户端通信异常：" + ex.Message);
            }
            finally
            {
                client.Close();
                string removeUID = null;
                lock (locker)
                {
                    clientList.Remove(client);
                    foreach (var kvp in uidToClient)
                    {
                        if (kvp.Value == client) { removeUID = kvp.Key; break; }
                    }
                    if (removeUID != null) uidToClient.Remove(removeUID);
                }
                if (removeUID != null)
                {
                    this.Invoke(new Action(() =>
                    {
                        listServerMessage.Items.Add($"用户 {removeUID} 断开连接");
                        UpdateOnlineUsers();
                    }));
                }
            }

        }

        private async Task SendMessageAsync(NetworkStream stream, string message)
        {
            byte[] body = Encoding.UTF8.GetBytes(message);
            byte[] length = BitConverter.GetBytes(body.Length);
            byte[] toSend = new byte[4 + body.Length];
            Buffer.BlockCopy(length, 0, toSend, 0, 4);
            Buffer.BlockCopy(body, 0, toSend, 4, body.Length);
            await stream.WriteAsync(toSend, 0, toSend.Length);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                cts?.Cancel();       // 触发取消
                server?.Stop();      // 停止监听
            }
            catch (Exception ex)
            {
                listServerMessage.Items.Add("关闭服务器时发生错误：" + ex.Message);
            }
        }


        private void UpdateOnlineUsers()
        {
            List<string> currentUIDs;
            lock (locker)
            {
                currentUIDs = uidToClient.Keys.ToList();
            }

            try
            {
                this.Invoke(new Action(() =>
                {
                    listOnlineUsers.Items.Clear();
                    foreach (var uid in currentUIDs)
                    {
                        listOnlineUsers.Items.Add(uid);
                    }
                }));
            }
            catch (Exception ex)
            {
                MessageBox.Show("更新在线用户失败：" + ex.Message);
            }
        }
        private bool ValidateUID(string uid)
        {
            Regex uidRegex = new Regex(@"^\d{6}$");
            return uidRegex.IsMatch(uid);
        }

        private void listOnlineUsers_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadServerConfig();
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

    }
}