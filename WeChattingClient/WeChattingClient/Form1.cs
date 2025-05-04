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
using J3QQ4;
using Google.Protobuf.WellKnownTypes;

namespace WeChattingClient
{
    public partial class Form1 : Form
    {
        //当前账号  密码 姓名
        private string Myaccount;
        private string MyPassword;
        private static string MyName;

        private static TcpClient client;
        private static NetworkStream stream;

        //是否运行
        private bool isRunning;
        //是否收到新消息
        bool receiveNews = false;
        //收到消息的人的姓名和消息
        string friendName;
        string chatInfo;

        //请求连接数据库
        private static string connectstring = "data source=localhost;database=wechatting;" +
     "user id=root;password=123456;pooling=true;charset=utf8;";
        //好友列表UID+姓名
        Dictionary<string, string> friend = new Dictionary<string, string>();
        Dictionary<string, string> friendChatInfo = new Dictionary<string, string>();
        //当前聊天对象UID，用于显示聊天内容并且正确的把聊天内容发送给对应对象
        private static string chatFriend = "";

        //当前聊天界面
        private Panel curPanel;
        //定时获得好友列表
        private System.Windows.Forms.Timer friendTimer;
        private void StartFriendListAutoRefresh()
        {
            friendTimer = new System.Windows.Forms.Timer();
            friendTimer.Interval = 1000; // 每1秒刷新一次
            friendTimer.Tick += (s, e) => RefreshFriendList();
            friendTimer.Start();
        }

        private void RefreshFriendList()
        {
            try
            {
                Dictionary<string, string> newFriendDict = new Dictionary<string, string>();

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
                            newFriendDict[friendUID] = friendName;
                        }
                    }
                }

                // 若有变化，则更新UI
                if (!newFriendDict.OrderBy(k => k.Key).SequenceEqual(friend.OrderBy(k => k.Key)))
                {
                    friend = newFriendDict;

                    listFriend.Items.Clear();
                    foreach (string fname in friend.Values)
                    {
                        ListViewItem item = new ListViewItem(fname);
                        listFriend.Items.Add(item);
                    }

                    listFriend.Items.Add(new ListViewItem("群聊")); // 确保群聊项也存在
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("实时刷新好友列表失败：" + ex.Message);
            }
            // 若当前未初始化聊天对象且好友列表不为空，则初始化聊天面板
            if (string.IsNullOrEmpty(chatFriend) && listFriend.Items.Count > 0)
            {
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

                ShowChatBox(kvpSearch); // 初始化面板
            }
        }
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
            MyName = myname;


            client = new TcpClient();
            client.Connect("127.0.0.1", 8888); // 连接服务器
            stream = client.GetStream();

            // 发送注册上线信息
            string registerMessage = "######" + "$你好$" + Myaccount;
            byte[] regBytes = Encoding.UTF8.GetBytes(registerMessage);
            stream.Write(regBytes, 0, regBytes.Length);

            //轮询获得好友列表
            StartFriendListAutoRefresh();


            isRunning = true;
            Thread receiveThread = new Thread(ReceiveMessages);
            receiveThread.IsBackground = true;
            receiveThread.Start();
        }
        #endregion


        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            throw new NotImplementedException();
        }
        #region 加载历史记录
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

                            panelReceive.Controls.Clear(); // 清除旧控件，避免重复加载

                            while (reader.Read())
                            {
                                string sender = reader.GetString("sender");
                                string message = reader.GetString("message");
                                DateTime sendTime = reader.GetDateTime("send_time");

                                AddMessageToPanel(friendName, sender, message, sendTime);
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
        #region 接收信息线程
        private void ReceiveMessages()
        {
            byte[] buffer = new byte[4096];
            StringBuilder messageBuilder = new StringBuilder();

            Console.WriteLine("[客户端] 接收线程启动");

            while (isRunning)
            {
                try
                {
                    if (!stream.DataAvailable)
                    {
                        Thread.Sleep(100);
                        continue;
                    }

                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        string part = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        messageBuilder.Append(part);

                        string full = messageBuilder.ToString();
                        int newlineIndex;

                        // 处理所有完整消息
                        while ((newlineIndex = full.IndexOf("\n")) != -1)
                        {
                            string oneMessage = full.Substring(0, newlineIndex).Trim();  // 一条完整消息
                            messageBuilder.Remove(0, newlineIndex + 1);  // 移除已处理部分
                            full = messageBuilder.ToString();

                            Console.WriteLine($"[客户端] 收到消息：{oneMessage}");

                            // 拆解消息：内容$sender$receiver
                            int first = oneMessage.IndexOf("$");
                            int second = oneMessage.IndexOf("$", first + 1);

                            if (first == -1 || second == -1) continue;

                            string message = oneMessage.Substring(0, first);
                            string senderUID = oneMessage.Substring(first + 1, second - first - 1);
                            string receiverUID = oneMessage.Substring(second + 1);

                            if (receiverUID == "000000" && senderUID == Myaccount)
                                continue;  // 自己发的群聊就不显示

                            HandleReceivedMessage(message, senderUID, receiverUID);
                        }
                    }
                    else
                    {
                        Console.WriteLine("[客户端] 服务器断开连接");
                        break;
                    }
                }
                catch (IOException ioEx)
                {
                    Console.WriteLine("[客户端] IO异常：" + ioEx.Message);
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[客户端] 接收异常：" + ex.ToString());
                }
            }
        }


        #endregion
        private void HandleReceivedMessage(string message, string senderUID, string receiverUID)
        {
            string displayName = (receiverUID == "000000")
                ? "群聊"
                : (senderUID == Myaccount
                    ? (friend.ContainsKey(receiverUID) ? friend[receiverUID] : receiverUID)
                    : (friend.ContainsKey(senderUID) ? friend[senderUID] : senderUID));

            this.Invoke(new Action(() =>
            {
                // 创建面板
                Control[] chatBoxes = panelChat.Controls.Find(displayName, false);
                if (chatBoxes.Length == 0)
                {
                    ShowChatBox(displayName);
                }

                // 文件消息处理
                if (message.StartsWith("FILE:"))
                {
                    try
                    {
                        string[] parts = message.Substring(5).Split('#');
                        if (parts.Length >= 3)
                        {
                            string fileName = parts[0];
                            string base64Data = parts[2];

                            byte[] fileBytes = Convert.FromBase64String(base64Data);
                            string saveDir = Path.Combine(Application.StartupPath, "downloads");
                            if (!Directory.Exists(saveDir)) Directory.CreateDirectory(saveDir);

                            string fullPath = Path.Combine(saveDir, fileName);
                            File.WriteAllBytes(fullPath, fileBytes);

                            MessageBox.Show($"收到文件：{fileName}\n已保存至：{fullPath}", "文件接收成功");
                            AddMessageToPanel(displayName, senderUID, $"收到文件：{fileName}", DateTime.Now);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("解析文件失败：" + ex.Message);
                        AddMessageToPanel(displayName, senderUID, "[收到损坏的文件]", DateTime.Now);
                    }
                }
                else
                {
                    AddMessageToPanel(displayName, senderUID, message, DateTime.Now);
                }
            }));
        }

        private void AddMessageToPanel(string friendName, string senderUID, string message, DateTime time)
        {
            Panel panelReceive = panelChat.Controls.Find(friendName, false).FirstOrDefault() as Panel;
            if (panelReceive == null) return;

            int currentY = panelReceive.Controls.Count > 0
                ? panelReceive.Controls.Cast<Control>().Max(c => c.Bottom) + 10
                : 10;

            // === 时间标签 ===
            Label timeLabel = new Label();
            timeLabel.Text = $"[{time:yyyy-MM-dd HH:mm:ss}]";
            timeLabel.AutoSize = true;
            timeLabel.Font = new Font("Arial", 9, FontStyle.Italic);
            timeLabel.ForeColor = Color.White;
            timeLabel.BackColor = Color.Gray;
            timeLabel.Padding = new Padding(6, 2, 6, 2);
            timeLabel.TextAlign = ContentAlignment.MiddleCenter;
            timeLabel.Left = (panelReceive.Width - timeLabel.PreferredWidth) / 2;
            timeLabel.Top = currentY;

            panelReceive.Controls.Add(timeLabel);
            currentY += timeLabel.Height + 5;

            // === 显示消息框 ===
            TextBox textBox = new TextBox();
            textBox.Multiline = true;
            textBox.ReadOnly = true;
            textBox.BorderStyle = BorderStyle.None;
            textBox.Font = new Font("Arial", 11, FontStyle.Bold);
            textBox.BackColor = senderUID == Myaccount ? Color.LightBlue : Color.LightGreen;
            textBox.ForeColor = senderUID == Myaccount ? Color.LightCoral : Color.Black;
            textBox.TextAlign = HorizontalAlignment.Left;

            // 显示文本内容（群聊则显示发送人）
            string displayText = message;
            if (friendName == "群聊" && senderUID != Myaccount)
            {
                string name = friend.ContainsKey(senderUID) ? friend[senderUID] : GetUserNameByUID(senderUID);
                displayText = $"{senderUID}（{name}）\r\n{message}";
            }

            textBox.Text = displayText;

            // 宽度设置为 50~60%（最多不超过宽度的一半），自动换行
            int maxWidth = panelReceive.Width * 3 / 5;
            textBox.Width = maxWidth;

            // 估算高度
            int lineCount = displayText.Split('\n').Length + displayText.Length / (textBox.Width / 10);
            int baseHeight = textBox.Font.Height;
            textBox.Height = Math.Max(baseHeight * lineCount + 10, 50);

            textBox.Top = currentY;
            textBox.Left = senderUID == Myaccount
                ? panelReceive.Width - textBox.Width - 10
                : 10;

            panelReceive.Controls.Add(textBox);

            // 自动滚动到底部
            panelReceive.ScrollControlIntoView(textBox);
        }


        private string GetUserNameByUID(string uid)
        {
            string result = uid; // 默认返回 UID
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectstring))
                {
                    conn.Open();
                    string sql = "SELECT username FROM user WHERE uid = @uid LIMIT 1";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", uid);
                        object usernameObj = cmd.ExecuteScalar();
                        if (usernameObj != null && usernameObj != DBNull.Value)
                        {
                            result = usernameObj.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("查询昵称失败：" + ex.Message);
            }
            return result;
        }



        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            isRunning = false;
            stream?.Close();
            client?.Close();
            client?.Dispose();
            Application.Exit();
        }
        #region 发消息事件
        private void buttonSend_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textSend.Text))
                return;

            string message = textSend.Text.Trim();
            textSend.Text = "";

            try
            {
                // 这里 chatFriend 本身就是 UID（从 listFriend 切换时赋值）
                string receiverUID = chatFriend == "群聊" ? "000000" : chatFriend;

                string sendMessage = receiverUID + "$" + message + "$" + Myaccount;
                byte[] sendBytes = Encoding.UTF8.GetBytes(sendMessage);

                if (stream != null && stream.CanWrite)
                {
                    stream.Write(sendBytes, 0, sendBytes.Length);
                }
                else
                {
                    MessageBox.Show("发送失败：网络连接已断开。");
                    return;
                }

                // === 本地显示消息 ===
                string displayName = (receiverUID == "000000") ? "群聊" : friend.ContainsKey(receiverUID) ? friend[receiverUID] : receiverUID;
                AddMessageToPanel(displayName, Myaccount, message, DateTime.Now);
            }
            catch (Exception ex)
            {
                MessageBox.Show("发送消息失败：" + ex.Message);
            }
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
            label1.Text = Emoji.Open_Mouth;
            label2.Text = Emoji.Hushed;
            label3.Text = Emoji.Grimacing;
            label4.Text = Emoji.Neutral_Face;
            label5.Text = Emoji.Sunglasses;
            label6.Text = Emoji.Angry;
            // 去掉灰线 + 设置背景透明一致

            listAdd.BorderStyle = BorderStyle.None;
            listAdd.BackColor = this.BackColor; // 或 Color.White
            listAdd.ForeColor = Color.Black;
            listAdd.Font = new Font("微软雅黑", 10);

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
            }
            else
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
            if (string.IsNullOrWhiteSpace(textBox1.Text) || !File.Exists(textBox1.Text))
            {
                MessageBox.Show("文件不存在");
                return;
            }

            try
            {
                string filePath = textBox1.Text;
                string fileName = Path.GetFileName(filePath);
                byte[] fileData = File.ReadAllBytes(filePath);
                string base64Data = Convert.ToBase64String(fileData);  // ✔️ 使用 Base64 编码

                // 格式: chatFriend$FILE:fileName#length#base64data$Myaccount
                string sendMessage = chatFriend + "$" + "FILE:" + fileName + "#" + fileData.Length + "#" + base64Data + "$" + Myaccount;
                byte[] sendBytes = Encoding.UTF8.GetBytes(sendMessage);

                if (stream != null && stream.CanWrite)
                {
                    stream.Write(sendBytes, 0, sendBytes.Length);
                    MessageBox.Show("发送成功");
                }
                else
                {
                    MessageBox.Show("连接已关闭，发送失败");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("发送失败：" + ex.Message);
            }
        }


        private void label1_Click_1(object sender, EventArgs e)
        {
            textSend.Text += label1.Text;
        }

        private void label2_Click(object sender, EventArgs e)
        {
            textSend.Text += label2.Text;
        }

        private void label3_Click(object sender, EventArgs e)
        {
            textSend.Text += label3.Text;
        }

        private void label4_Click(object sender, EventArgs e)
        {
            textSend.Text += label4.Text;
        }

        private void label5_Click(object sender, EventArgs e)
        {
            textSend.Text += label5.Text;
        }

        private void label6_Click(object sender, EventArgs e)
        {
            textSend.Text += label6.Text;
        }
    }
}