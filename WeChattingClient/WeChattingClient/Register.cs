using System;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace WeChattingClient
{
    public partial class Register : Form
    {

        //请求连接数据库
        private static string connectstring = DbConfig.GetConnectionString();

        public Register()
        {
            InitializeComponent();
            this.FormClosing += Register_FormClosing;
            GenerateUniqueUID(); // 自动生成不重复UID
                                 
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
        }

        private void Register_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        // 自动生成唯一账号（UID）
        private void GenerateUniqueUID()
        {
            Random rand = new Random();
            string uid;
            bool exists = true;

            using (MySqlConnection conn = new MySqlConnection(connectstring))
            {
                conn.Open();
                while (exists)
                {
                    uid = rand.Next(100000, 999999).ToString(); // 生成6位数字
                    string checkSql = "SELECT COUNT(*) FROM userinfo WHERE UID = @uid";
                    using (MySqlCommand cmd = new MySqlCommand(checkSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", uid);
                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        if (count == 0)
                        {
                            textAccount.Text = uid;
                            exists = false;
                        }
                    }
                }
            }
        }

        private TcpClient client;
        private NetworkStream stream;

        private void buttonRegister_Click(object sender, EventArgs e)
        {
            string uid = textAccount.Text.Trim();
            string name = textName.Text.Trim();
            string pwd1 = textPassword1.Text;
            string pwd2 = textPassword2.Text;

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("姓名不能为空");
                return;
            }
            if (pwd1.Length == 0 || pwd2.Length == 0)
            {
                MessageBox.Show("密码不能为空");
                return;
            }
            if (pwd1 != pwd2)
            {
                MessageBox.Show("两次密码不一致");
                return;
            }

            try
            {
                // 建立连接
                if (client == null || !client.Connected)
                {
                    client = new TcpClient("127.0.0.1", 8888);
                    stream = client.GetStream();
                }

                // 构造注册消息
                string registerMsg = $"REGISTER${uid}${pwd1}${name}";
                byte[] body = Encoding.UTF8.GetBytes(registerMsg);
                byte[] length = BitConverter.GetBytes(body.Length);
                byte[] message = new byte[4 + body.Length];
                Buffer.BlockCopy(length, 0, message, 0, 4);
                Buffer.BlockCopy(body, 0, message, 4, body.Length);
                stream.Write(message, 0, message.Length);

                // === 使用长度头读取响应 ===
                byte[] lenBuf = new byte[4];
                int lenRead = stream.Read(lenBuf, 0, 4);
                if (lenRead != 4)
                {
                    MessageBox.Show("读取响应长度失败", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

                // === 响应处理 ===
                if (response.StartsWith("REGISTER_OK"))
                {
                    MessageBox.Show("注册成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Form1 userForm = new Form1(uid, pwd1, name, client, stream);
                    userForm.Show();
                    this.Hide();
                }
                else if (response.StartsWith("REGISTER_FAIL$"))
                {
                    string err = response.Substring("REGISTER_FAIL$".Length);
                    MessageBox.Show("注册失败：" + err, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show("服务端返回未知响应：" + response, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("注册失败：" + ex.Message, "异常", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 最小化按钮
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        // 关闭按钮
        private void pictureBox2_Click(object sender, EventArgs e)
        {
            Application.ExitThread();
        }

        // 空方法占位符（设计器需要）
        private void label1_Click(object sender, EventArgs e) { }
        private void textPassword1_TextChanged(object sender, EventArgs e) { }
        private void label4_Click(object sender, EventArgs e) { }
        private void textPassword2_TextChanged(object sender, EventArgs e) { }
        private void label3_Click(object sender, EventArgs e) { }
    }
}