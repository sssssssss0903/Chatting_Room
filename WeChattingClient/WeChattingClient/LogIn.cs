using System;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace WeChattingClient
{
    public partial class LogIn : Form
    {
        // 连接字符串（连接本地数据库 wechatting）
        private static readonly string ConnectionString = "server=localhost;database=wechatting;" +
            "user id=root;password=123456;pooling=true;charset=utf8;";

        public LogIn()
        {
            InitializeComponent();
        }

        // 登录按钮点击事件
        private void buttonLogIn_Click(object sender, EventArgs e)
        {
            // 获取用户输入
            string account = textInputAccount.Text.Trim();
            string password = textInputPassword.Text.Trim();

            // 空值判断
            if (string.IsNullOrEmpty(account) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("用户名或密码不能为空", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // 建立数据库连接
                using (MySqlConnection conn = new MySqlConnection(ConnectionString))
                {
                    // SQL语句使用参数化防止SQL注入
                    string sql = "SELECT UID, Password, UserName FROM userinfo WHERE UID = @account AND Password = @password";

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@account", account);
                        cmd.Parameters.AddWithValue("@password", password);

                        conn.Open(); // 打开连接

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            // 如果找到用户记录
                            if (reader.Read())
                            {
                                string name = reader.GetString("UserName");

                                // 登录成功，跳转主界面
                                Form1 userForm = new Form1(account, password, name);

                                userForm.Show();
                                this.Hide(); // 隐藏登录窗口
                                return;
                            }
                        }
                    }

                    // 如果 reader.Read() 没有命中账号密码，则登录失败
                    MessageBox.Show("账号或密码错误", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                // 捕获数据库连接或执行过程中的异常
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
    }
}