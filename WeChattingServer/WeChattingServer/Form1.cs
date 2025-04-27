using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace WeChattingServer
{
    public partial class Form1 : Form
    {
        //端口号（默认为8888）
       private int serverPort = 8888;
        //标识服务端地址，并且封装了Socket具有接收和发送消息的功能
        private UdpClient server;
       private static List<IPEndPoint> clientList = new List<IPEndPoint>();
       private Thread receiveThread;
        private Thread clearThread;
        //记录所有人的UID，地址信息
        private Dictionary<string, IPEndPoint> identify = new Dictionary<string, IPEndPoint>();
        public Form1()
        {
            InitializeComponent();
            receiveThread = new Thread(() =>
            {
                while (true)
                {
                    //创建一个地址来接收客户端
                    IPEndPoint clientEP = new IPEndPoint(IPAddress.Any, 0);
                    byte[] receiveBytes = server.Receive(ref clientEP);
                    Console.WriteLine("新收到消息" + Encoding.UTF8.GetString(receiveBytes));

                    string receiveMessage = Encoding.UTF8.GetString(receiveBytes);

                    int index = receiveMessage.IndexOf("$");
                    int lastIndex = receiveMessage.LastIndexOf("$");
                    string sendInfo = receiveMessage.Substring(0, index);
                    string receInfo = receiveMessage.Substring(lastIndex + 1);
                    string sendMessage = receiveMessage.Remove(0, index + 1);
                    byte[] sendBytes = Encoding.UTF8.GetBytes(sendMessage);

                    // ⭐ 改成用Invoke
                    this.Invoke(new Action(() =>
                    {
                        listServerMessage.Items.Add($"收到来自{clientEP}的消息:{receiveMessage}");
                    }));

                    if (sendInfo.Equals("000000"))
                    {
                        foreach (IPEndPoint ep in clientList)
                        {
                            if (ep != clientEP)
                            {
                                server.Send(sendBytes, sendBytes.Length, ep);
                            }
                        }
                    }
                    else
                    {
                        if (sendInfo.Equals("######") && !clientList.Contains(clientEP))
                        {
                            clientList.Add(clientEP);

                            this.Invoke(new Action(() =>
                            {
                                listServerMessage.Items.Add($"添加新客户端:{clientEP}");
                            }));

                            identify.Add(receInfo, clientEP);
                            continue;
                        }

                        IPEndPoint ep;
                        if (identify.TryGetValue(sendInfo, out ep) && ep != null)
                        {
                            server.Send(sendBytes, sendBytes.Length, ep);
                        }
                        else
                        {
                            // 如果找不到目标ep，可以打印日志或者跳过
                            this.Invoke(new Action(() =>
                            {
                                listServerMessage.Items.Add($"警告：找不到发送对象UID={sendInfo}对应的地址！");
                            }));
                        }
                    }
                }
            });

            clearThread = new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(10000);
                    lock (clientList)
                    {
                        for (int i = clientList.Count - 1; i >= 0; i--)
                        {
                            IPEndPoint ep = clientList[i];
                            try
                            {
                                server.Send(new byte[0], 0, ep);
                            }
                            catch
                            {
                                this.Invoke(new Action(() =>
                                {
                                    listServerMessage.Items.Add($"用户【{ep}】离开聊天室");
                                }));

                                clientList.RemoveAt(i);

                                byte[] sendBytes = Encoding.UTF8.GetBytes($"用户 {ep} 离开聊天室");
                                foreach (IPEndPoint ep2 in clientList)
                                {
                                    server.Send(sendBytes, sendBytes.Length, ep2);
                                }
                            }
                        }
                    }
                }
            });

            receiveThread.IsBackground = true;
            clearThread.IsBackground = true;
        }
        //创建一个线程来接收来自客户端的信息,并发送给其他客户端
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            receiveThread?.Abort();
            clearThread?.Abort();
            server?.Close();
        }
        private void buttonListen_Click(object sender, EventArgs e)
        {
            //如果输入端口号
            if(this.textServerPort.Text.Trim()!="")
            {
                serverPort = Convert.ToInt32(this.textServerPort.Text);
            }
            EndPoint serverEP = new IPEndPoint(IPAddress.Any, serverPort);
            server = new UdpClient(serverPort);
            listServerMessage.Items.Add("服务端启动成功");
            if (receiveThread.ThreadState == ThreadState.Running)
            {
                // 线程正在运行中
            }
            else
            {
                receiveThread.Start();
            }
            if (clearThread.ThreadState == ThreadState.Running)
            {
                // 线程正在运行中
            }
            else
            {
                clearThread.Start();
            }
           
        }
    }
}
