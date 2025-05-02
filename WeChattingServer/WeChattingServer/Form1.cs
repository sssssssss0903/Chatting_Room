using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
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
        private string connStr = "server=localhost;user id=root;password=123456;database=wechatting;charset=utf8;";

        public Form1()
        {
            InitializeComponent();
            this.FormClosing += Form1_FormClosing;
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
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                        break;

                    string msg = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine("收到消息: " + msg);

                    int firstIndex = msg.IndexOf("$");
                    int lastIndex = msg.LastIndexOf("$");

                    if (firstIndex == -1 || lastIndex == -1 || firstIndex == lastIndex)
                        continue;

                    string receiverUID = msg.Substring(0, firstIndex);                    // 接收者 UID
                    string messageBody = msg.Substring(firstIndex + 1, lastIndex - firstIndex - 1); // 消息内容
                    string senderUID = msg.Substring(lastIndex + 1);                     // 发送者 UID

                    this.Invoke(new Action(() =>
                    {
                        listServerMessage.Items.Add($"[来自 {senderUID} 发给 {receiverUID}]：{messageBody}");
                    }));

                    string fullMsg = messageBody + "$" + senderUID + "$" + receiverUID;
                    byte[] toSend = Encoding.UTF8.GetBytes(fullMsg);

                    // 保存聊天记录到数据库
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
                    if (receiverUID == "######")
                    {
                        // 注册上线消息
                        lock (locker)
                        {
                            if (!uidToClient.ContainsKey(senderUID))
                            {
                                uidToClient[senderUID] = client;
                                clientList.Add(client);
                                this.Invoke(new Action(() =>
                                {
                                    listServerMessage.Items.Add($"新用户上线: {senderUID}");
                                }));
                            }
                            else
                            {
                                uidToClient[senderUID] = client; // 更新连接
                            }
                        }
                        continue;
                    }

                    if (receiverUID == "000000")
                    {
                        // 群聊广播（除自己外都发）
                        lock (locker)
                        {
                            foreach (var other in clientList)
                            {
                                if (other != client && other.Connected)
                                {
                                    try
                                    {
                                        other.GetStream().Write(toSend, 0, toSend.Length);
                                    }
                                    catch
                                    {

                                    }
                                   
                                }
                            }
                        }
                    }
                    else
                    {
                        // 私聊
                        lock (locker)
                        {
                            if (uidToClient.TryGetValue(receiverUID, out TcpClient targetClient) && targetClient.Connected)
                            {
                                try
                                {
                                    targetClient.GetStream().Write(toSend, 0, toSend.Length);
                                }
                                catch { }
                            }
                            else
                            {
                                this.Invoke(new Action(() =>
                                {
                                    listServerMessage.Items.Add($" 未找到 UID={receiverUID} 的在线客户端");
                                }));
                            }
                        }
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
                lock (locker)
                {
                    clientList.Remove(client);
                    string removeUID = null;

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
                        this.Invoke(new Action(() =>
                        {
                            listServerMessage.Items.Add($"用户 {removeUID} 断开连接");
                        }));
                    }
                }
            }
        }


        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            server?.Stop();
            listenThread?.Abort();
        }
    }
}
