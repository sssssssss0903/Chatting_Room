using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

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

        public Form1()
        {
            InitializeComponent();
            this.FormClosing += Form1_FormClosing;
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

                // 构建连接字符串
                connStr = $"server={host};user id={user};password={password};database={dbName};charset={charset};";
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

            listenThread = new Thread(ListenForClients);
            listenThread.IsBackground = true;
            listenThread.Start();
        }

        private void ListenForClients()
        {
            while (true)
            {
                try
                {
                    TcpClient client = server.AcceptTcpClient();
                    clientList.Add(client);

                    Thread clientThread = new Thread(() => HandleClientComm(client));
                    clientThread.IsBackground = true;
                    clientThread.Start();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("监听客户端连接异常：" + ex.Message);
                    break;
                }
            }
        }

        private void HandleClientComm(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[4096];

            try
            {
                while (true)
                {
                    // === 第一步：读取前4个字节作为长度头 ===
                    byte[] lengthBuffer = new byte[4];
                    int lengthRead = stream.Read(lengthBuffer, 0, 4);
                    if (lengthRead != 4) break; // 客户端断开

                    int messageLength = BitConverter.ToInt32(lengthBuffer, 0);
                    if (messageLength <= 0 || messageLength > 10_000_000) break; // 防止恶意数据（最多10MB）

                    // === 第二步：按长度读取消息体 ===
                    byte[] messageBuffer = new byte[messageLength];
                    int totalRead = 0;
                    while (totalRead < messageLength)
                    {
                        int read = stream.Read(messageBuffer, totalRead, messageLength - totalRead);
                        if (read == 0) break; // 客户端断开
                        totalRead += read;
                    }

                    if (totalRead != messageLength) break; // 异常中断

                    string msg = Encoding.UTF8.GetString(messageBuffer);
                    Console.WriteLine("收到消息: " + msg);

                    // === 拆分消息 ===
                    // 引入命名分组的正则表达式
                    Regex msgRegex = new Regex(@"^(?<receiver>[#\w]+)\$(?<message>.*?)\$(?<sender>\w+)$");



                    Match match = msgRegex.Match(msg);
                    if (!match.Success)
                    {
                        Console.WriteLine("消息格式非法，忽略");
                        return;
                    }

                    string receiverUID = match.Groups["receiver"].Value;
                    string messageBody = match.Groups["message"].Value;
                    string senderUID = match.Groups["sender"].Value;

                    if (!ValidateUID(senderUID))
                    {
                        Console.WriteLine($"UID 格式不合法（{senderUID}），已拦截");
                        return;
                    }

                    
                    if (receiverUID != "000000" && receiverUID != "######" && !ValidateUID(receiverUID))
                    {
                        Console.WriteLine($"接收者 UID 非法（{receiverUID}），已拦截");
                        return;
                    }
                    // === 注册上线消息（不会转发） ===
                    if (receiverUID == "######")
                    {
                        bool isNew;

                        lock (locker)
                        {
                            isNew = !uidToClient.ContainsKey(senderUID);
                            uidToClient[senderUID] = client;
                            if (!clientList.Contains(client))
                                clientList.Add(client);
                        }

                        // 这里已经脱离 lock 范围，可以放心调用 Invoke
                        this.Invoke(new Action(() =>
                        {
                            if (isNew)
                                listServerMessage.Items.Add($"新用户上线: {senderUID}");

                            UpdateOnlineUsers(); 
                        }));
                        continue;
                    }

                    // === 服务端显示 ===
                    this.Invoke(new Action(() =>
                    {
                        listServerMessage.Items.Add($"[来自 {senderUID} 发给 {receiverUID}]：{messageBody}");
                    }));

                    // === 构造转发消息（带长度头） ===
                    string forwardMsg = messageBody + "$" + senderUID + "$" + receiverUID;
                    byte[] forwardBytes = Encoding.UTF8.GetBytes(forwardMsg);
                    byte[] forwardLength = BitConverter.GetBytes(forwardBytes.Length);
                    byte[] toSend = new byte[4 + forwardBytes.Length];
                    Buffer.BlockCopy(forwardLength, 0, toSend, 0, 4);
                    Buffer.BlockCopy(forwardBytes, 0, toSend, 4, forwardBytes.Length);

                    // === 群聊/私聊 转发 ===
                    if (receiverUID == "000000")
                    {
                        lock (locker)
                        {
                            foreach (var other in clientList.ToList())
                            {
                                if (other.Connected)
                                {
                                    try
                                    {
                                        other.GetStream().Write(toSend, 0, toSend.Length);
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine("群聊发送失败: " + ex.Message);
                                    }
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
                                try
                                {
                                    targetClient.GetStream().Write(toSend, 0, toSend.Length);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("私聊发送失败: " + ex.Message);
                                }
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
                    // === 存入数据库 ===
                    try
                    {
                        using (MySqlConnection conn = new MySqlConnection(connStr))
                        {
                            conn.Open();
                            string sql = "INSERT INTO chatinfo (sender, receiver, message, send_time) VALUES (@sender, @receiver, @message, NOW())";
                            using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                            {
                                cmd.Parameters.AddWithValue("@sender", senderUID);
                                cmd.Parameters.AddWithValue("@receiver", receiverUID);
                                cmd.Parameters.AddWithValue("@message", messageBody);
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("保存消息失败：" + ex.Message);
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
                        if (kvp.Value == client)
                        {
                            removeUID = kvp.Key;
                            break;
                        }
                    }

                    if (removeUID != null)
                    {
                        uidToClient.Remove(removeUID);
                    }
                }

                // 在 lock 外调用 UI 更新，避免死锁
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



        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            server?.Stop();
            listenThread?.Abort();
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