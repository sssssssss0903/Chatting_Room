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

        public LogIn()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            textInputPassword.UseSystemPasswordChar = true;
            LoadLoginFromXml();
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
                // 保证连接状态干净
                TcpConnectionManager.Close();
                if (!TcpConnectionManager.Connect())
                {
                    MessageBox.Show("连接服务器失败", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 构造登录消息并发送
                string loginMsg = $"LOGIN${account}${password}";
                byte[] body = Encoding.UTF8.GetBytes(loginMsg);
                byte[] length = BitConverter.GetBytes(body.Length);
                byte[] message = new byte[4 + body.Length];
                Buffer.BlockCopy(length, 0, message, 0, 4);
                Buffer.BlockCopy(body, 0, message, 4, body.Length);
                TcpConnectionManager.Send(message);

                // 接收响应长度
                byte[] lenBuf = new byte[4];
                TcpConnectionManager.Read(lenBuf, 0, 4);
                int respLen = BitConverter.ToInt32(lenBuf, 0);
                if (respLen <= 0 || respLen > 10000)
                {
                    MessageBox.Show("响应长度非法", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 接收响应体
                byte[] respBody = new byte[respLen];
                TcpConnectionManager.Read(respBody, 0, respLen);
                string response = Encoding.UTF8.GetString(respBody);

                if (response.StartsWith("LOGIN_OK$"))
                {
                    string[] parts = response.Split('$');
                    if (parts.Length >= 2)
                    {
                        string userName = parts[1];

                        // 发送上线标识
                        string onlineMsg = $"######$你好${account}";
                        byte[] onlineBody = Encoding.UTF8.GetBytes(onlineMsg);
                        byte[] onlineLen = BitConverter.GetBytes(onlineBody.Length);
                        byte[] onlinePacket = new byte[4 + onlineBody.Length];
                        Buffer.BlockCopy(onlineLen, 0, onlinePacket, 0, 4);
                        Buffer.BlockCopy(onlineBody, 0, onlinePacket, 4, onlineBody.Length);
                        TcpConnectionManager.Send(onlinePacket);

                        // 打开主窗口
                        Form1 userForm = new Form1(account, password, userName);
                        userForm.Show();
                        this.Hide();
                    }
                    else
                    {
                        MessageBox.Show("登录响应格式错误", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else if (response.StartsWith("LOGIN_FAIL2"))
                {
                    MessageBox.Show("该账号已在其他设备登录", "登录失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

        private void buttonRegister_Click(object sender, EventArgs e)
        {
            Register registerForm = new Register();
            registerForm.Show();
            this.Hide();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            Application.ExitThread();
        }

        private void textInputPassword_TextChanged(object sender, EventArgs e) { }
        private void textInputAccount_TextChanged(object sender, EventArgs e) { }
        private void LogIn_Load(object sender, EventArgs e) { }

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
