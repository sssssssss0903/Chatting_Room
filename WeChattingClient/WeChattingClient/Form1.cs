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
        private static string MyName;

        private static UdpClient client;
        private  int port;
        //是否运行
        private bool isRunning;
        //是否收到新消息
        bool receiveNews=false;
        //收到消息的人的姓名和消息
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
        public Form1(string myaccount, string mypassword, string myname)
        {
            InitializeComponent();
            this.FormClosing += Form1_FormClosing;

            Myaccount = myaccount;
            MyPassword = mypassword;
            isRunning = true;
            timer = new System.Windows.Forms.Timer();
            timer.Interval = 1000; // 1秒检查一次是否有新消息
            timer.Tick += ShowDia;
            timer.Start();

            // 获取好友列表
            try
            {
                string sqlFriend = "SELECT FriendUID, UIDName FROM friend WHERE Myaccount = @uid";
                using (MySqlConnection mscFriend = new MySqlConnection(connectstring))
                {
                    MySqlCommand cmdFriend = new MySqlCommand(sqlFriend, mscFriend);
                    cmdFriend.Parameters.AddWithValue("@uid", Myaccount);
                    mscFriend.Open();
                    using (MySqlDataReader readerFriend = cmdFriend.ExecuteReader())
                    {
                        while (readerFriend.Read())
                        {
                            string friendUID = readerFriend.GetString(0);
                            string friendName = readerFriend.GetString(1);
                            friend.Add(friendUID, friendName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("数据库操作失败：" + ex.Message);
            }

            MyName = myname;

            // 显示好友列表
            foreach (string fname in friend.Values)
            {
                ListViewItem item = new ListViewItem(fname);
                this.listFriend.Items.Add(item);
            }

            // 群聊
            ListViewItem itemGroup = new ListViewItem("群聊");
            listFriend.Items.Add(itemGroup);

            // 默认先显示第一个聊天对象
            string kvpSearch = listFriend.Items[0].Text;

            if (kvpSearch == "群聊")
            {
                chatFriend = "000000"; // 群聊特殊UID
            }
            else
            {
                foreach (KeyValuePair<string, string> kvp in friend)
                {
                    if (kvp.Value.Equals(kvpSearch))
                    {
                        chatFriend = kvp.Key;
                        break;
                    }
                }
            }

            // 初始化聊天框
            Panel chatBox = new Panel();
            chatBox.AutoScroll = true;
            chatBox.Name = kvpSearch;
            chatBox.BackColor = Color.White;
            chatBox.Dock = DockStyle.Fill;

            string imagePath = @"..\Resources\bkgend.png";
            string directoryPath = Path.GetDirectoryName(Application.StartupPath);
            string fullPath = Path.Combine(directoryPath, imagePath);
            if (File.Exists(fullPath))
            {
                chatBox.BackgroundImage = Image.FromFile(fullPath);
                chatBox.BackgroundImageLayout = ImageLayout.Stretch;
            }

            curPanel = chatBox;
            panelChat.Controls.Add(chatBox);

            //  加载历史聊天记录
            LoadHistoryMessages(kvpSearch);
        }



        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            throw new NotImplementedException();
        }
        private void LoadHistoryMessages(string friendName)
        {
            string sql = "";
            string friendUID = "";

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectstring))
                {
                    conn.Open();

                    if (friendName == "群聊")
                    {
                        friendUID = "000000";
                        sql = $"SELECT sender, receiver, message, send_time FROM chatinfo WHERE receiver = '000000' ORDER BY send_time ASC";
                    }
                    else
                    {
                        // 从 friend 字典中找到对应UID
                        if (!friend.TryGetValue(friendName, out friendUID))
                        {
                            friendUID = friend.FirstOrDefault(x => x.Value == friendName).Key;
                        }
                        sql = $"SELECT sender, receiver, message, send_time FROM chatinfo WHERE (sender=@me AND receiver=@friend) OR (sender=@friend AND receiver=@me) ORDER BY send_time ASC";
                    }

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        if (friendName != "群聊")
                        {
                            cmd.Parameters.AddWithValue("@me", Myaccount);
                            cmd.Parameters.AddWithValue("@friend", friendUID);
                        }

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            Panel panelReceive = panelChat.Controls.Find(friendName, false).FirstOrDefault() as Panel;
                            if (panelReceive == null) return;

                            int index = 0;
                            while (reader.Read())
                            {
                                string sender = reader.GetString("sender");
                                string receiver = reader.GetString("receiver");
                                string message = reader.GetString("message");
                                DateTime sendTime = reader.GetDateTime("send_time");

                                TextBox textBox = new TextBox();
                                textBox.Multiline = true;
                                textBox.ReadOnly = true;
                                textBox.BorderStyle = BorderStyle.None;
                                textBox.Font = new Font("Arial", 12, FontStyle.Bold);
                                textBox.Width = panelReceive.Width / 3;

                                // 文本内容：时间换行+内容
                                string showText;
                                if (friendName == "群聊" && sender != Myaccount)
                                {
                                    showText = $"[{sendTime:yyyy-MM-dd HH:mm:ss}]\r\n{sender}: {message}";
                                }
                                else
                                {
                                    showText = $"[{sendTime:yyyy-MM-dd HH:mm:ss}]\r\n{message}";
                                }
                                textBox.Text = showText;

                                // 动态计算高度（更自然换行）
                                int baseHeight = textBox.Font.Height;
                                int lineCount = showText.Split('\n').Length;
                                lineCount += showText.Length / (textBox.Width / 10); // 简单估算太长一行也换行
                                textBox.Height = Math.Max(baseHeight * lineCount + 10, 50);

                                // 设置 Top
                                int controlCount = panelReceive.Controls.OfType<TextBox>().Count();
                                int yOffset = 10;
                                textBox.Top = controlCount * (textBox.Height + yOffset);

                                // 设置颜色和左右位置
                                if (sender == Myaccount)
                                {
                                    textBox.BackColor = Color.LightBlue;
                                    textBox.TextAlign = HorizontalAlignment.Left;
                                    textBox.Left = panelReceive.Width - textBox.Width - 5;
                                    textBox.ForeColor = Color.LightCoral;
                                }
                                else
                                {
                                    textBox.BackColor = Color.LightGreen;
                                    textBox.TextAlign = HorizontalAlignment.Left;
                                    textBox.Left = 5;
                                    textBox.ForeColor = Color.Black;
                                }

                                panelReceive.Controls.Add(textBox);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("加载历史聊天记录失败：" + ex.Message);
            }
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
            if (string.IsNullOrWhiteSpace(textSend.Text))
                return; // 防止发送空消息

            // 发送信息，格式: 目标UID$text内容$自己UID
            string sendMessage = chatFriend + "$" + textSend.Text + "$" + Myaccount;
            byte[] sendBytes = Encoding.UTF8.GetBytes(sendMessage);

            client.Send(sendBytes, sendBytes.Length, new IPEndPoint(IPAddress.Parse("10.138.179.108"), 8888));

            // 写入统一的 chatinfo 表，带发送时间
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectstring))
                {
                    conn.Open();
                    string sqlInsert = "INSERT INTO chatinfo (sender, receiver, message, send_time) VALUES (@sender, @receiver, @message, @send_time)";
                    using (MySqlCommand cmd = new MySqlCommand(sqlInsert, conn))
                    {
                        cmd.Parameters.AddWithValue("@sender", Myaccount);    // 自己是发送者
                        cmd.Parameters.AddWithValue("@receiver", chatFriend); // 接收者可以是好友UID或者"000000"群聊
                        cmd.Parameters.AddWithValue("@message", textSend.Text);
                        cmd.Parameters.AddWithValue("@send_time", DateTime.Now); // 当前时间
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("发送消息失败：" + ex.Message);
            }

            // 显示自己发送的消息
            TextBox textBox = new TextBox();
            textBox.Multiline = true;
            textBox.ReadOnly = true;
            textBox.BorderStyle = BorderStyle.None;
            textBox.Font = new Font("Arial", 12, FontStyle.Bold);
            textBox.Width = curPanel.Width / 3;
            textBox.BackColor = Color.LightBlue;
            textBox.TextAlign = HorizontalAlignment.Left;
            textBox.ForeColor = Color.LightCoral;
            textBox.Left = curPanel.Width - textBox.Width - 5;

            // 文本内容：时间 + 换行 + 内容
            string showText = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]\r\n{textSend.Text}";
            textBox.Text = showText;

            // 计算高度（动态）
            int baseHeight = textBox.Font.Height;
            int lineCount = showText.Split('\n').Length;
            lineCount += showText.Length / (textBox.Width / 10); // 字符太多也需要换行
            textBox.Height = Math.Max(baseHeight * lineCount + 10, 50);

            // 位置调整
            int controlCount = curPanel.Controls.OfType<TextBox>().Count();
            int yOffset = 10;
            textBox.Top = controlCount * (textBox.Height + yOffset);

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
            byte[] sendBytes = Encoding.UTF8.GetBytes($"######${MyName}】请求加入聊天室${Myaccount}");

            try
            {
                client.Send(sendBytes, sendBytes.Length, new IPEndPoint(IPAddress.Parse("10.138.179.108"), 8888));
                listMessage.Items.Add("成功加入聊天");
            }
            catch (Exception ex)
            {
                MessageBox.Show("发送加入请求失败：" + ex.Message);
            }
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
                // 先关闭当前打开的聊天框
                foreach (Panel p in panelChat.Controls)
                {
                    if (p.Visible == true)
                        p.Visible = false;
                }

                // 获取点击的好友名字
                ListViewItem selectFriend = listFriend.SelectedItems[0];
                string friendName = selectFriend.Text;

                // 根据名字修改chatFriend
                if (friendName.Equals("群聊"))
                {
                    chatFriend = "000000"; // 群聊UID固定
                }
                else
                {
                    foreach (KeyValuePair<string, string> kvp in friend)
                    {
                        if (kvp.Value.Equals(friendName))
                            chatFriend = kvp.Key;
                    }
                }

                // 切换聊天框
                ShowChatBox(friendName);
            }
        }

        #endregion
        #region 切换对话框，在每次切换聊天好友时调用
        //显示与好友的对话框
        public void ShowChatBox(string friendName)
        {
            Control[] existingChatBoxes = panelChat.Controls.Find(friendName, false);

            if (existingChatBoxes.Length > 0) // 已存在聊天框
            {
                existingChatBoxes[0].Visible = true;
                curPanel = (Panel)existingChatBoxes[0];
            }
            else
            {
                // 创建新的聊天框
                Panel chatBox = new Panel();
                chatBox.AutoScroll = true;
                chatBox.Name = friendName;
                chatBox.BackColor = Color.White;
                chatBox.Dock = DockStyle.Fill;

                string imagePath = @"..\Resources\bkgend.png";
                string directoryPath = Path.GetDirectoryName(Application.StartupPath);
                string fullPath = Path.Combine(directoryPath, imagePath);

                if (File.Exists(fullPath))
                {
                    chatBox.BackgroundImage = Image.FromFile(fullPath);
                    chatBox.BackgroundImageLayout = ImageLayout.Stretch;
                }

                curPanel = chatBox;
                panelChat.Controls.Add(chatBox);

                //  新建完后，加载聊天历史
                LoadHistoryMessages(friendName);
            }
        }

        #endregion
        #region 接收信息线程
        private void ReceiveMessages()
        {
            while (isRunning)
            {
                try
                {
                    IPEndPoint serverEP = new IPEndPoint(IPAddress.Any, 0);
                    byte[] receiveBytes = client.Receive(ref serverEP);

                    if (receiveBytes != null && receiveBytes.Length > 0)
                    {
                        Console.WriteLine("收到消息");

                        string receiveMessage = Encoding.UTF8.GetString(receiveBytes);

                        // 拆分聊天内容和发送者身份
                        int lastIndex = receiveMessage.LastIndexOf("$");
                        if (lastIndex == -1) continue; // 格式错误，跳过

                        chatInfo = receiveMessage.Substring(0, lastIndex);
                        string friendInfo = receiveMessage.Substring(lastIndex + 1);

                        // 保存到统一chatinfo表
                        using (MySqlConnection conn = new MySqlConnection(connectstring))
                        {
                            conn.Open();
                            string sql = "INSERT INTO chatinfo (sender, receiver, message, send_time) VALUES (@sender, @receiver, @message, @send_time)";
                            using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                            {
                                cmd.Parameters.AddWithValue("@sender", friendInfo);
                                cmd.Parameters.AddWithValue("@receiver", Myaccount);
                                cmd.Parameters.AddWithValue("@message", chatInfo);
                                cmd.Parameters.AddWithValue("@send_time", DateTime.Now); // 当前时间
                                cmd.ExecuteNonQuery();
                            }
                        }

                        // 更新聊天面板
                        if (!friend.TryGetValue(friendInfo, out friendName))
                        {
                            if (friendInfo == "000000")
                                friendName = "群聊";
                            else
                                friendName = friendInfo; // 临时好友（直接UID）
                        }

                        receiveNews = true;
                    }
                }
                catch (SocketException ex)
                {
                    Console.WriteLine("ReceiveMessages Socket异常（正常关闭）：" + ex.Message);
                    break; // 退出线程
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ReceiveMessages 其他异常：" + ex.Message);
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

                if (existingChatBoxes.Length > 0)
                {
                    Panel panelReceive = (Panel)existingChatBoxes[0];

                    try
                    {
                        using (MySqlConnection conn = new MySqlConnection(connectstring))
                        {
                            conn.Open();

                            // 从 chatinfo 里查询最近一条消息（发送人是 friendName，对象是自己）
                            string sql = "SELECT message, send_time FROM chatinfo WHERE sender = @sender AND receiver = @receiver ORDER BY send_time DESC LIMIT 1";
                            using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                            {
                                if (friendName == "群聊")
                                {
                                    cmd.Parameters.AddWithValue("@sender", "000000"); // 群聊发送者
                                    cmd.Parameters.AddWithValue("@receiver", Myaccount);
                                }
                                else
                                {
                                    cmd.Parameters.AddWithValue("@sender", friend.FirstOrDefault(x => x.Value == friendName).Key);
                                    cmd.Parameters.AddWithValue("@receiver", Myaccount);
                                }

                                using (MySqlDataReader reader = cmd.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        string message = reader.GetString("message");
                                        DateTime sendTime = reader.GetDateTime("send_time");

                                        // 先添加时间标签
                                        Label timeLabel = new Label();
                                        timeLabel.Text = sendTime.ToString("yyyy-MM-dd HH:mm:ss");
                                        timeLabel.AutoSize = true;
                                        timeLabel.Font = new Font("Arial", 9, FontStyle.Italic);
                                        timeLabel.ForeColor = Color.Gray;
                                        timeLabel.Top = panelReceive.Controls.OfType<Control>().Count() * 50;
                                        timeLabel.Left = panelReceive.Width / 2 - 50; // 居中大概的位置
                                        panelReceive.Controls.Add(timeLabel);

                                        // 再添加消息框
                                        TextBox textBox = new TextBox();
                                        textBox.Text = message;
                                        textBox.Multiline = true;
                                        textBox.ReadOnly = true;
                                        textBox.BorderStyle = BorderStyle.None;
                                        textBox.BackColor = Color.LightGreen; // 接收到的消息绿色区分
                                        textBox.TextAlign = HorizontalAlignment.Left;
                                        textBox.Font = new Font("Arial", 12, FontStyle.Regular);
                                        textBox.ForeColor = Color.Black;
                                        textBox.Padding = new Padding(5);

                                        textBox.Width = panelReceive.Width / 3;
                                        textBox.Top = timeLabel.Top + 20; // 比时间往下挪一点
                                        textBox.Left = 5;

                                        panelReceive.Controls.Add(textBox);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("显示新消息失败：" + ex.Message);
                    }

                    receiveNews = false;
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
            string searchUID = this.textAdd.Text.Trim();

            if (string.IsNullOrEmpty(searchUID))
            {
                MessageBox.Show("请输入要搜索的UID");
                return;
            }

            this.listAdd.Items.Clear(); // 清空之前的搜索结果

            string sqlUID = "SELECT UID, UserName FROM userinfo WHERE UID = @uid";

            using (MySqlConnection mscUID = new MySqlConnection(connectstring))
            {
                MySqlCommand cmdUID = new MySqlCommand(sqlUID, mscUID);
                cmdUID.Parameters.AddWithValue("@uid", searchUID);
                mscUID.Open();
                using (MySqlDataReader readerUID = cmdUID.ExecuteReader())
                {
                    if (readerUID.Read())
                    {
                        string name = readerUID.GetString(1);
                        this.listAdd.Items.Add(name);
                    }
                    else
                    {
                        this.listAdd.Items.Add("不存在此人");
                    }
                }
            }
        }


        private void buttonAddFriend_Click(object sender, EventArgs e)
        {
            string searchFriendUID = this.textAdd.Text.Trim();
            if (string.IsNullOrEmpty(searchFriendUID))
            {
                MessageBox.Show("请输入对方UID");
                return;
            }

            if (searchFriendUID == Myaccount)
            {
                MessageBox.Show("不能添加自己为好友");
                return;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectstring))
                {
                    conn.Open();

                    // 先检查是否已是好友
                    string checkSql = "SELECT COUNT(*) FROM friend WHERE Myaccount = @uid AND FriendUID = @friend";
                    using (MySqlCommand checkCmd = new MySqlCommand(checkSql, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@uid", Myaccount);
                        checkCmd.Parameters.AddWithValue("@friend", searchFriendUID);
                        int count = Convert.ToInt32(checkCmd.ExecuteScalar());
                        if (count > 0)
                        {
                            MessageBox.Show("已添加为好友");
                            return;
                        }
                    }

                    // 查询对方昵称
                    string getNameSql = "SELECT UserName FROM userinfo WHERE UID = @uid";
                    string friendName = "";
                    using (MySqlCommand cmd = new MySqlCommand(getNameSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", searchFriendUID);
                        var reader = cmd.ExecuteReader();
                        if (reader.Read())
                        {
                            friendName = reader.GetString(0);
                        }
                        else
                        {
                            MessageBox.Show("用户不存在");
                            return;
                        }
                        reader.Close();
                    }

                    // 添加好友关系（互相）
                    string insertSql = "INSERT INTO friend (Myaccount, FriendUID, UIDName) VALUES (@uid, @friend, @name)";
                    using (MySqlCommand insertCmd1 = new MySqlCommand(insertSql, conn))
                    {
                        insertCmd1.Parameters.AddWithValue("@uid", Myaccount);
                        insertCmd1.Parameters.AddWithValue("@friend", searchFriendUID);
                        insertCmd1.Parameters.AddWithValue("@name", friendName);
                        insertCmd1.ExecuteNonQuery();
                    }

                    using (MySqlCommand insertCmd2 = new MySqlCommand(insertSql, conn))
                    {
                        insertCmd2.Parameters.AddWithValue("@uid", searchFriendUID);
                        insertCmd2.Parameters.AddWithValue("@friend", Myaccount);
                        insertCmd2.Parameters.AddWithValue("@name", MyName);
                        insertCmd2.ExecuteNonQuery();
                    }

                    // 本地更新好友
                    friend.Add(searchFriendUID, friendName);
                    ListViewItem item = new ListViewItem(friendName);
                    listFriend.Items.Add(item);

                    // 新建聊天面板
                    Panel chatBox = new Panel();
                    chatBox.AutoScroll = true;
                    chatBox.Name = friendName;
                    chatBox.BackColor = Color.White;
                    chatBox.Dock = DockStyle.Fill;
                    chatBox.Visible = false;

                    string imagePath = @"..\Resources\bkgend.png";
                    string directoryPath = Path.GetDirectoryName(Application.StartupPath);
                    string fullPath = Path.Combine(directoryPath, imagePath);
                    if (File.Exists(fullPath))
                    {
                        chatBox.BackgroundImage = Image.FromFile(fullPath);
                        chatBox.BackgroundImageLayout = ImageLayout.Stretch;
                    }

                    panelChat.Controls.Add(chatBox);

                    MessageBox.Show("添加好友成功！");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("添加好友失败：" + ex.Message);
            }
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

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        public void uploadFile()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;//该值确定是否可以选择多个文件
            dialog.Title = "请选择文件夹";
            dialog.Filter = "所有文件(*.*)|*.*";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string file = dialog.FileName;
                textBox1.Text = file;
            } else
            {
                MessageBox.Show("选择文件失败");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            uploadFile();

        }

        private void button2_Click(object sender, EventArgs e)
        {

        }
    }
}
