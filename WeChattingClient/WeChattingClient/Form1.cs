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
using MySql.Data.MySqlClient;
using System.IO;

namespace WeChattingClient
{
    public partial class Form1 : Form
    {
        //当前账号  密码 姓名
        private string Myaccount;
        private string MyPassword;
        private static string name;

        private static UdpClient client;
        private  int port;
        //是否运行
        private bool isRunning;
        //是否收到新消息
        bool receiveNews=false;
        //收到消息的人的姓名和消息！！！不一定是当前正在聊天的对象
        string friendName;
        string chatInfo;
        private System.Windows.Forms.Timer timer;
        //请求连接数据库
        private static string connectstring = "data source=localhost;database=wechatting;" +
     "user id=root;password=123456;pooling=true;charset=utf8;";
        //好友列表UID+姓名
        Dictionary<string, string> friend = new Dictionary<string, string>();
        Dictionary<string, string> friendChatInfo = new Dictionary<string, string>();
        //当前聊天对象UID，用于显示聊天内容并且正确的把聊天内容发送给对应对象
        private static string chatFriend = " ";
        //是否搜索到好友
        private bool haveSearched = false;
        //当前聊天界面
        private Panel curPanel;
        public Form1()
        {
            InitializeComponent();
        }
        #region 带参构造函数
        public Form1(string myaccount,string mypassword ,string myname)
        {

            InitializeComponent();
            this.FormClosing += Form1_FormClosing;
            Myaccount = myaccount;
            MyPassword = mypassword;
            isRunning = true;
            timer = new System.Windows.Forms.Timer();
            timer.Interval = 1000; // 1秒检查一次是否有新消息
            timer.Tick += ShowDia; // 设置定时器的事件处理程序

            // 启动定时器
            timer.Start();
            //获取好友列表
            try
            {
                string sqlFriend = "select Friend,Name from friend where UID=" + Myaccount;
                MySqlConnection mscFriend = new MySqlConnection(connectstring);
                MySqlCommand cmdFriend = new MySqlCommand(sqlFriend, mscFriend);
                //开启读数据库
                mscFriend.Open();
                MySqlDataReader readerFriend = cmdFriend.ExecuteReader();
                while (readerFriend.Read())
                {
                    string friendUID = readerFriend.GetString(0);

                    string friendName = readerFriend.GetString(1);
                    friend.Add(friendUID, friendName);
                }
                readerFriend.Close();
                mscFriend.Close();
            }
            catch
            {
                MessageBox.Show("数据库操作失败");
            }
            name = myname;
         
            //显示好友列表
            foreach( string fname in friend.Values)
            {
                // 创建一个 ListViewItem 对象
                ListViewItem item = new ListViewItem(fname); // 在第一列添加数据
                // 将 ListViewItem 添加到 ListView 的 Items 集合中
                this.listFriend.Items.Add(item);
            }

            //群聊

            ListViewItem itemGroup = new ListViewItem("群聊");
            listFriend.Items.Add(itemGroup);
            //默认先显示第一个好友的聊天内容
            string kvpSearch= listFriend.Items[0].Text;
            chatFriend = kvpSearch;
            foreach (KeyValuePair<string,string> kvp in friend)
            {
                if (kvp.Value.Equals(kvpSearch))
                {
                    //获取当前聊天对象UID
                    chatFriend = kvp.Key;
                }
            }
            //初始化聊天框
            Panel chatBox = new Panel();
            chatBox.AutoScroll = true;
            chatBox.Name = kvpSearch;
            chatBox.BackColor = Color.White;
            chatBox.Dock = DockStyle.Fill;
            string imagePath = @"..\Resources\bkgend.png";
            string directoryPath = Path.GetDirectoryName(Application.StartupPath);
            string fullPath = Path.Combine(directoryPath, imagePath);
            chatBox.BackgroundImage = Image.FromFile(fullPath);
            chatBox.BackgroundImageLayout = ImageLayout.Stretch;
            curPanel = chatBox;
            // 将聊天框添加到容器中
            panelChat.Controls.Add(chatBox);
           
           
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            throw new NotImplementedException();
        }
        #endregion
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            isRunning = false;
            client?.Close();
            client?.Dispose();
            Application.Exit();
        }
        #region 发消息事件

        private void buttonSend_Click(object sender, EventArgs e)
        {
            //发送信息时，要附带对方和自己的身份识别信息；
            string sendMessage = chatFriend+"$"+textSend.Text+"$"+Myaccount;
            byte[] sendBytes = Encoding.UTF8.GetBytes(sendMessage);
            client.Send(sendBytes, sendBytes.Length, new IPEndPoint(IPAddress.Parse("10.133.201.218"), 8888));
            string sqlAddFriendMessage = "insert into " + Myaccount + "chatinfo values(@value1,@value2,@value3)";
            MySqlConnection mscaddFriendMessage = new MySqlConnection(connectstring);
            mscaddFriendMessage.Open();
           MySqlCommand cmdAddFriendMessage = new MySqlCommand(sqlAddFriendMessage, mscaddFriendMessage);
            cmdAddFriendMessage.Parameters.AddWithValue("@value1", Myaccount);
            cmdAddFriendMessage.Parameters.AddWithValue("@value2", chatFriend);
            cmdAddFriendMessage.Parameters.AddWithValue("@value3", textSend.Text);
            cmdAddFriendMessage.ExecuteNonQuery();
            cmdAddFriendMessage.Dispose();
            mscaddFriendMessage.Close();
            //显示自己信息
            TextBox textBox = new TextBox();
            textBox.Top = curPanel.Controls.OfType<TextBox>().Count() * 40;
            textBox.Text = textSend.Text;
            textBox.Multiline = true;
            textBox.ReadOnly = true;
            textBox.BorderStyle = BorderStyle.None;
            textBox.BackColor = Color.LightBlue;
            textBox.TextAlign = HorizontalAlignment.Left;
            textBox.Font = new Font("Arial", 12, FontStyle.Bold);
            textBox.ForeColor = Color.LightCoral;

            // 设置 textBox 的大小和位置
            textBox.Width = curPanel.Width/3; // 根据实际需求设置宽度
            textBox.Left= curPanel.Width-5-textBox.Width; // 根据实际需求设置左侧边距
            // 添加到聊天面板
            curPanel.Controls.Add(textBox);
            textSend.Text = "";

        }
        #endregion
        #region 初次请求连接
        //连接
        //点击登录时调用
        public async void ConnectInfo()
        {
            Random rd = new Random();
            this.port = rd.Next(1025, 8888);
            //本地地址
            client = new UdpClient(new IPEndPoint(IPAddress.Any, port));
            //发送加入请求  表明身份
            byte[] sendBytes = Encoding.UTF8.GetBytes($"######"+"$"+"{name}】请求加入聊天室"+"$"+Myaccount);
            //找到主机并请求加入聊天
            client.Send(sendBytes, sendBytes.Length, new IPEndPoint(IPAddress.Parse("10.133.201.218"), 8888));
            listMessage.Items.Add("成功加入聊天");
            //开始接收
            await Task.Run(() => ReceiveMessages());

        }
        #endregion
        #region 切换聊天对象
        //点击好友姓名事件
        private void listFriend_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listFriend.SelectedItems.Count > 0)
            {
                //先关闭当前已经打开的聊天框
                foreach (Panel p in panelChat.Controls)
                {
                    if (p.Visible == true)
                        p.Visible = false;
                }
                //获取点击的好友姓名
                ListViewItem selectFriend = listFriend.SelectedItems[0];
                string friendName = selectFriend.Text;
                //更改目前正在聊天的好友UID
                if (friendName.Equals("群聊"))
                {
                    chatFriend = "000000";//如果是群聊，则UID为000000
                }
                else
                {
                    foreach (KeyValuePair<string, string> kvp in friend)
                    {
                        if (kvp.Value.Equals(friendName))
                            chatFriend = kvp.Key;
                    }
                }
                ShowChatBox(friendName);
            }
        }
        #endregion
        #region 切换对话框，在每次切换聊天好友时调用
        //显示与好友的对话框
        public void ShowChatBox(string friendName)
        {
            Control[] existingChatBoxes = panelChat.Controls.Find(friendName, false);
            if(existingChatBoxes.Length>0)//已经存在此好友的聊天框控件
            {
                existingChatBoxes[0].Visible = true; // 显示已存在的聊天框
                curPanel = (Panel)existingChatBoxes[0];
            }
            else
            {

                // 创建新的聊天
                Panel chatBox = new Panel();              
                chatBox.AutoScroll = true;
                chatBox.Name = friendName;
                chatBox.BackColor = Color.White;
                chatBox.Dock = DockStyle.Fill;
                string imagePath = @"..\Resources\bkgend.png";
                string directoryPath = Path.GetDirectoryName(Application.StartupPath);
                string fullPath = Path.Combine(directoryPath, imagePath);
                chatBox.BackgroundImage = Image.FromFile(fullPath);
                chatBox.BackgroundImageLayout = ImageLayout.Stretch;
                curPanel = chatBox;

                // 将聊天框添加到容器中
                panelChat.Controls.Add(chatBox);
            }

        }
        #endregion
        #region 接收信息线程
        private void ReceiveMessages()
        {
            while (isRunning)
            {
                IPEndPoint serverEP = new IPEndPoint(IPAddress.Any, 0);
                //阻塞
                byte[] receiveBytes = client.Receive(ref serverEP);
                Console.WriteLine("收到消息");
                string receiveMessage = Encoding.UTF8.GetString(receiveBytes);
                if (receiveMessage.Length > 0)
                {
                    //聊天内容
                    int lastIndex = receiveMessage.LastIndexOf("$");

                    chatInfo = receiveMessage.Substring(0,lastIndex);
                    //朋友身份信息
                    string friendInfo = receiveMessage.Substring(lastIndex +1 );
                    string sqlAddFriendMessage = "insert into " + Myaccount + "chatinfo values(@value1,@value2,@value3)";
                    MySqlConnection mscaddFriendMessage = new MySqlConnection(connectstring);
                    mscaddFriendMessage.Open();
                    MySqlCommand cmdAddFriendMessage = new MySqlCommand(sqlAddFriendMessage, mscaddFriendMessage);
                    cmdAddFriendMessage.Parameters.AddWithValue("@value1",friendInfo);
                    cmdAddFriendMessage.Parameters.AddWithValue("@value2", Myaccount);
                    cmdAddFriendMessage.Parameters.AddWithValue("@value3", chatInfo);
                    cmdAddFriendMessage.ExecuteNonQuery();
                    cmdAddFriendMessage.Dispose();
                    mscaddFriendMessage.Close();
                    //根据发送来的消息选择添加到哪个聊天框
                    //1.根据UID-》Name-》Panel
                    friend.TryGetValue(friendInfo, out friendName);
                   
                    receiveNews = true;
                }
            }
        }
        #endregion
        #region 显示新信息，由时钟事件触发
        public void ShowDia(object sender, EventArgs e)
        {
            if (receiveNews)
            {
                Control[] existingChatBoxes = panelChat.Controls.Find(friendName, false);
                //listMessage.Items.Add(receiveMessage);
                if (existingChatBoxes.Length > 0)
                {
                    Panel panelReceive = (Panel)existingChatBoxes[0];
                    TextBox textBox = new TextBox();

                    textBox.Text = chatInfo;
                    textBox.Multiline = true;
                    textBox.ReadOnly = true;
                    textBox.BorderStyle = BorderStyle.None;
                    textBox.BackColor = Color.LightBlue;
                    textBox.TextAlign = HorizontalAlignment.Left;
                    textBox.Font = new Font("Arial", 12, FontStyle.Bold);
                    textBox.ForeColor = Color.LightCoral;
                    textBox.Top = curPanel.Controls.OfType<TextBox>().Count() * 40;
                    // 设置 textBox 的大小和位置
                    textBox.Width = panelReceive.Width/3; // 根据实际需求设置宽度
                    textBox.Left = 5; // 根据实际需求设置左侧边距

                    // 添加到聊天面板
                    panelReceive.Controls.Add(textBox);


                    receiveNews = false;//等待新消息来
                }
            }
        }
        #endregion

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            this.buttonAddFriend.Visible = true;
            this.buttonSureAdd.Visible = true;
            this.labelAdd.Visible = true;
            this.textAdd.Visible = true;
            this.listAdd.Visible = true;
            this.buttonReturn.Visible = true;
            this.buttonAdd.Visible = false;
        }

        private void buttonReturn_Click(object sender, EventArgs e)
        {
            this.buttonSureAdd.Visible = false;
            this.labelAdd.Visible = false;
            this.textAdd.Visible = false;
            this.listAdd.Visible = false;
            this.buttonReturn.Visible = false;
            this.buttonAddFriend.Visible = false;
            this.buttonAdd.Visible = true;
        }

        private void buttonSureAdd_Click(object sender, EventArgs e)
        {
            bool isSearched = false;
            string searchUID = this.textAdd.Text;
            string sqlUID = "select UID,UserName from userinfo where UID=" + searchUID;
            MySqlConnection mscUID = new MySqlConnection(connectstring);
            MySqlCommand cmdUID = new MySqlCommand(sqlUID, mscUID);
            mscUID.Open();
            MySqlDataReader readerUID = cmdUID.ExecuteReader();
            while(readerUID.Read())
            {
                string UID = readerUID.GetString(0);
                if(searchUID.Equals(UID))
                {
                    string name = readerUID.GetString(1);
                    this.listAdd.Items.Add(name);
                    haveSearched = true;
                    isSearched = true;
                }
            }
            if(!isSearched)
            {
                this.listAdd.Items.Add("不存在此人");
            }
            readerUID.Close();
            mscUID.Close();
        }

        private void buttonAddFriend_Click(object sender, EventArgs e)
        {
            string searchFriendUID = this.textAdd.Text;
            string sqlAddFriend1 = "insert into friend values(@value1,@value2,@value3)";
            string sqlAddFriend2 = "insert into friend values(@value1,@value2,@value3)";
            string sqlGetName = "select UserName from userinfo where UID=" + searchFriendUID;
            MySqlConnection mscaddFriend = new MySqlConnection(connectstring);
            MySqlCommand cmdAddFriend1 = new MySqlCommand(sqlAddFriend1, mscaddFriend);
            MySqlCommand cmdAddFriend2 = new MySqlCommand(sqlAddFriend2, mscaddFriend);
            //获得好友姓名
            MySqlCommand cmdGetName = new MySqlCommand(sqlGetName, mscaddFriend);
            mscaddFriend.Open();
            MySqlDataReader readerName = cmdGetName.ExecuteReader();
            string UIDName;
            while (readerName.Read())
            {
                UIDName = readerName.GetString(0);
                readerName.Close();
                cmdAddFriend1.Parameters.AddWithValue("@value1", Myaccount);
                cmdAddFriend1.Parameters.AddWithValue("@value2", searchFriendUID);
                cmdAddFriend1.Parameters.AddWithValue("@value3", UIDName);
                cmdAddFriend1.ExecuteNonQuery();
                cmdAddFriend1.Dispose();

                cmdAddFriend2.Parameters.AddWithValue("@value1", searchFriendUID);
                cmdAddFriend2.Parameters.AddWithValue("@value2", Myaccount);
                cmdAddFriend2.Parameters.AddWithValue("@value3", name);
                cmdAddFriend2.ExecuteNonQuery();
                cmdAddFriend2.Dispose();

                friend.Add(searchFriendUID, UIDName);

                ListViewItem item = new ListViewItem(UIDName);
                this.listFriend.Items.Add(item);

                break;
            }
            mscaddFriend.Close();
        }

        private void panelChat_Paint(object sender, PaintEventArgs e)
        {

        }

        private void textSend_TextChanged(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void quit_Click(object sender, EventArgs e)
        {
            Application.ExitThread();
        }

        private void textAdd_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
