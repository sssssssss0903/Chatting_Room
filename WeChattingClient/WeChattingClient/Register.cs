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

    public partial class Register : Form
    {
        private string account;
        private static string connectstring = "data source=localhost;database=wechatting;" +
"user id=root;password=123456;pooling=true;charset=utf8;";
        MySqlConnection msc;
        MySqlCommand cmd;
        MySqlDataReader reader;
        public Register()
        {
            InitializeComponent();
            this.FormClosing += Register_FormClosing;
            //设置密码
            msc = new MySqlConnection(connectstring);
            string sqlGetUID = "select UID from userinfo";
            cmd = new MySqlCommand(sqlGetUID, msc);
            msc.Open();
            Random rd = new Random();
            Boolean same = false;
            do
            {
                account = rd.Next(1, 999999).ToString();
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string userUID = reader.GetString(0);
                    if (account.Equals(userUID))
                    {
                        same = true;
                        break;
                    }
                }
            } while (same);
            textAccount.Text = account;
            reader.Close();
        }
        private void Register_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }
        private void buttonRegister_Click(object sender, EventArgs e)
        {
            string password1 = textPassword1.Text;
            string password2 = textPassword2.Text;
            if(textName.Text.Trim().Length==0)
            {
                MessageBox.Show("姓名不能为空（doge）");
            }
            else if(!(password1.Equals(password2)))
            {
                MessageBox.Show("两次密码输入不一致，请重新输入");
            }
            else
            {
                string sqlUserInfo = "insert into userinfo (UID,Password,UserName) values (@value1,@value2,@value3)";
                string sqlCreatechatInfo = "create table " + textAccount.Text + "chatinfo( sender varchar(50)," +
                    "receiver varchar(50),message varchar(250)CHARACTER SET utf8)";
                MySqlCommand cmdUserInfo = new MySqlCommand(sqlUserInfo, msc);
                cmdUserInfo.Parameters.AddWithValue("@value1", textAccount.Text);
                cmdUserInfo.Parameters.AddWithValue("@value2", password1);
                cmdUserInfo.Parameters.AddWithValue("@value3", textName.Text);
              
                MySqlCommand cmdCreatechatInfo = new MySqlCommand(sqlCreatechatInfo, msc);
                cmdUserInfo.ExecuteNonQuery();
                cmdUserInfo.Dispose();
                cmdCreatechatInfo.ExecuteNonQuery();
                msc.Close();
                cmdCreatechatInfo.Dispose();
                Form1 userFrom = new Form1(textAccount.Text, password1, textName.Text);
                userFrom.ConnectInfo();
                userFrom.Show();
                this.Hide();
                return;
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void textPassword1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void textPassword2_TextChanged(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            Application.ExitThread();
        }
    }
}
