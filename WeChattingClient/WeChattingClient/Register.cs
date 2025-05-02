using System;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace WeChattingClient
{
    public partial class Register : Form
    {
        private static readonly string ConnectionString = "server=127.0.0.1;user id=root;password=123456;database=wechatting;Charset=utf8;";

        public Register()
        {
            InitializeComponent();
            this.FormClosing += Register_FormClosing;
            GenerateUniqueUID(); // 自动生成不重复UID
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

            using (MySqlConnection conn = new MySqlConnection(ConnectionString))
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

        // 注册按钮点击事件
        private void buttonRegister_Click(object sender, EventArgs e)
        {
            string uid = textAccount.Text.Trim();
            string name = textName.Text.Trim();
            string pwd1 = textPassword1.Text;
            string pwd2 = textPassword2.Text;

            // 表单验证
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
                using (MySqlConnection conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();

                    // 插入用户信息
                    string insertUser = "INSERT INTO userinfo (UID, Password, UserName) VALUES (@uid, @pwd, @name)";
                    using (MySqlCommand cmdInsert = new MySqlCommand(insertUser, conn))
                    {
                        cmdInsert.Parameters.AddWithValue("@uid", uid);
                        cmdInsert.Parameters.AddWithValue("@pwd", pwd1);
                        cmdInsert.Parameters.AddWithValue("@name", name);
                        cmdInsert.ExecuteNonQuery();
                    }

                    // 创建聊天表
                    string chatTableName = $"chatinfo_{uid}";
                    string createTableSql = $@"
                        CREATE TABLE IF NOT EXISTS `{chatTableName}` (
                            sender VARCHAR(50),
                            receiver VARCHAR(50),
                            message VARCHAR(250) CHARACTER SET utf8
                        )";
                    try
                    {
                        using (MySqlCommand cmdCreate = new MySqlCommand(createTableSql, conn))
                        {
                            cmdCreate.ExecuteNonQuery();
                        }
                    }
                    catch (Exception innerEx)
                    {
                        // 即使创建失败，也不影响登录，只提醒
                        MessageBox.Show("注册成功，但创建聊天表失败：" + innerEx.Message, "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        // 跳转主界面
                        Form1 userForm = new Form1(uid, pwd1, name);
                     
                        userForm.Show();
                        this.Hide();
                        return;
                    }

                    // 注册与建表都成功才提示
                    MessageBox.Show("注册成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // 跳转到聊天窗口
                    Form1 userFormFinal = new Form1(uid, pwd1, name);
                  
                    userFormFinal.Show();
                    this.Hide();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("注册失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
