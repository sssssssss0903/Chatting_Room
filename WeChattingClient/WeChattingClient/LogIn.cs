using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using MySql.Data.MySqlClient;

namespace WeChattingClient
{
    public partial class LogIn : Form
    {
        private static string connectstring = DbConfig.GetConnectionString();
        private static System.Net.Sockets.TcpClient client;
        private static NetworkStream stream;
        public LogIn()
        {   
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            LoadLoginFromXml();
            // 初始化 TCP 连接
            try
            {
                client = new TcpClient("127.0.0.1", 8888);
                stream = client.GetStream();
            }
            catch (Exception ex)
            {
                MessageBox.Show("无法连接服务器：" + ex.Message);
            }
        }

        private void buttonLogIn_Click(object sender, EventArgs e)
        {
            string account = textInputAccount.Text.Trim();
            string password = textInputPassword.Text.Trim();

            if (string.IsNullOrEmpty(account) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("用户名或密码不能为空", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // 1. 建立连接（如未连接）
                if (client == null || !client.Connected)
                {
                    string serverIP = "127.0.0.1"; 
                    int serverPort = 8888;
                    client = new TcpClient();
                    client.Connect(serverIP, serverPort);
                    stream = client.GetStream();
                }

                // 2. 构造登录请求
                string loginMsg = $"LOGIN${account}${password}";
                byte[] body = Encoding.UTF8.GetBytes(loginMsg);
                byte[] length = BitConverter.GetBytes(body.Length);
                byte[] message = new byte[4 + body.Length];
                Buffer.BlockCopy(length, 0, message, 0, 4);
                Buffer.BlockCopy(body, 0, message, 4, body.Length);

                stream.Write(message, 0, message.Length);

                // 3. 等待响应（先读4字节长度）
                byte[] lenBuf = new byte[4];
                int lenRead = stream.Read(lenBuf, 0, 4);
                if (lenRead != 4)
                {
                    MessageBox.Show("未能读取完整响应长度", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                int respLen = BitConverter.ToInt32(lenBuf, 0);
                if (respLen <= 0 || respLen > 10000)
                {
                    MessageBox.Show("响应长度非法", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                byte[] respBody = new byte[respLen];
                int totalRead = 0;
                while (totalRead < respLen)
                {
                    int r = stream.Read(respBody, totalRead, respLen - totalRead);
                    if (r == 0)
                    {
                        MessageBox.Show("连接中断", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    totalRead += r;
                }

                string response = Encoding.UTF8.GetString(respBody);

                // 4. 处理响应
                if (response.StartsWith("LOGIN_OK$"))
                {
                    string[] parts = response.Split('$');
                    if (parts.Length >= 2)
                    {
                        string userName = parts[1];

                        // 登录成功后发送上线标识
                        string onlineMsg = $"######$你好${account}";
                        byte[] onlineBody = Encoding.UTF8.GetBytes(onlineMsg);
                        byte[] onlineLen = BitConverter.GetBytes(onlineBody.Length);
                        byte[] onlinePacket = new byte[4 + onlineBody.Length];
                        Buffer.BlockCopy(onlineLen, 0, onlinePacket, 0, 4);
                        Buffer.BlockCopy(onlineBody, 0, onlinePacket, 4, onlineBody.Length);
                        stream.Write(onlinePacket, 0, onlinePacket.Length);

                        // 跳转主窗口
                        Form1 userForm = new Form1(account, password, userName, client, stream);
                        userForm.Show();
                        this.Hide();
                    }
                    else
                    {
                        MessageBox.Show("登录响应格式错误", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else if (response.StartsWith("LOGIN_FAIL"))
                {
                    MessageBox.Show("账号或密码错误", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show("未知响应：" + response, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("登录失败：" + ex.Message, "异常", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        // 注册按钮点击事件：跳转注册页面
        private void buttonRegister_Click(object sender, EventArgs e)
        { 
            Register registerForm = new Register();
            registerForm.Show();
            this.Hide();
        }

        // 最小化按钮事件
        private void pictureBox2_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        // 关闭按钮事件
        private void pictureBox3_Click(object sender, EventArgs e)
        {
            Application.ExitThread();
        }

        // 密码框文本改变事件（未启用逻辑）
        private void textInputPassword_TextChanged(object sender, EventArgs e)
        {
        }

        // 账号框文本改变事件（未启用逻辑）
        private void textInputAccount_TextChanged(object sender, EventArgs e)
        {
        }

        // 窗体加载事件（未启用逻辑）
        private void LogIn_Load(object sender, EventArgs e)
        {
        }
        private void LoadLoginFromXml()
        {
            try
            {
              
                string uid = DbConfig.UID;
                string pwd = DbConfig.Password;
             

                if (!string.IsNullOrEmpty(uid)) textInputAccount.Text = uid;
                if (!string.IsNullOrEmpty(pwd)) textInputPassword.Text = pwd;
            }
            catch (Exception ex)
            {
                MessageBox.Show("读取登录信息失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}