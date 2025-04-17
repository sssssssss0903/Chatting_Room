using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
namespace WeChattingClient
{
    public partial class LogIn : Form
    {
        private string Myaccount;
        private string MyPassword;
        private string MyName;
        private static string connectstring = "data source=localhost;database=wechatting;" +
            "user id=root;password=admin;pooling=true;charset=utf8;";
        private MySqlConnection msc = new MySqlConnection(connectstring);
        public LogIn()
        {
            InitializeComponent();
        }
        //登录检查
        private void buttonLogIn_Click(object sender, EventArgs e)
        {
            //获取用户名和密码
            Myaccount = this.textInputAccount.Text;
            MyPassword = this.textInputPassword.Text;
            if(Myaccount==""||MyPassword=="")
            {
                MessageBox.Show("输入用户名或密码为空，请重新输入");
            }
            try
            {
                //数据库连接
                string sql = "select * from userinfo";
                MySqlCommand cmd = new MySqlCommand(sql, msc);
                msc.Open();
                //读数据
                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    //账号正确,切换界面并传递数据
                    if (Myaccount.Equals(reader.GetString(0)) && MyPassword.Equals(reader.GetString(1)))
                    {
                        MyName = reader.GetString(2);
                        Console.WriteLine("正确");
                        //关闭数据库连接
                        reader.Close();
                        msc.Close();
                        //生成聊天窗口 
                        Form1 userFrom = new Form1(Myaccount, MyPassword, MyName);
                        userFrom.ConnectInfo();
                        userFrom.Show();
                        this.Hide();
                        return;
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            MessageBox.Show("账号或密码输入错误，请重新输入");

        }

        private void buttonRegister_Click(object sender, EventArgs e)
        {
            Register registerForm = new Register();
            registerForm.Show();
            this.Hide();
            return;
        }

        private void textInputPassword_TextChanged(object sender, EventArgs e)
        {

        }

        private void textInputAccount_TextChanged(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            Application.ExitThread();
        }

        private void LogIn_Load(object sender, EventArgs e)
        {

        }
    }
}
