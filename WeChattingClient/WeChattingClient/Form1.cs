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
using System.Text.RegularExpressions;
using System.Xml;

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

        //请求连接数据库
        private static string connectstring = DbConfig.GetConnectionString();
        //好友列表UID+姓名
        Dictionary<string, string> friend = new Dictionary<string, string>();
        Dictionary<string, string> friendChatInfo = new Dictionary<string, string>();
        //当前聊天对象UID，用于显示聊天内容并且正确的把聊天内容发送给对应对象
        private static string chatFriend = "";

        //当前聊天界面
        private Panel curPanel;
        //定时获得好友列表
        private System.Windows.Forms.Timer friendTimer;
        private ImageList friendImageList = new ImageList();
        // 标记哪些好友当前有新消息未读（显示红点）
        private HashSet<string> uidWithRedDot = new HashSet<string>();
        private void StartFriendListAutoRefresh()
        {
            friendTimer = new System.Windows.Forms.Timer();
            friendTimer.Interval = 1000; // 每1秒刷新一次
            friendTimer.Tick += (s, e) => RefreshFriendList();
            friendTimer.Start();
        }


        public Form1()
        {
            InitializeComponent();
          
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            typeof(ListView).InvokeMember("DoubleBuffered",
              System.Reflection.BindingFlags.SetProperty |
              System.Reflection.BindingFlags.Instance |
              System.Reflection.BindingFlags.NonPublic,
              null, listFriend, new object[] { true });


        }
        #region 带参构造函数
        public Form1(string myaccount, string mypassword, string myname, TcpClient myclient, NetworkStream mystream)
        {
            InitializeComponent();
            
            client = myclient;
          stream = mystream;

            this.FormClosing += Form1_FormClosing;

            Myaccount = myaccount;
            MyPassword = mypassword;
            MyName = myname;
            label7.Text = myname;

            // 发送注册上线信息
            string registerMessage = "######$$" + Myaccount;

            byte[] body = Encoding.UTF8.GetBytes(registerMessage);
            byte[] length = BitConverter.GetBytes(body.Length);

            byte[] toSend = new byte[4 + body.Length];
            Buffer.BlockCopy(length, 0, toSend, 0, 4);
            Buffer.BlockCopy(body, 0, toSend, 4, body.Length);

            stream.Write(toSend, 0, toSend.Length);

            //轮询获得好友列表
            StartFriendListAutoRefresh();


            isRunning = true;
            Thread receiveThread = new Thread(ReceiveMessages);
            receiveThread.IsBackground = true;
            receiveThread.Start();

            // 禁止改变窗体大小
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;




            if (listFriend.Columns.Count == 0)
                listFriend.Columns.Add("好友", listFriend.Width - 20);

            friendImageList.ImageSize = new Size(64, 64); // 控制头像显示大小
            friendImageList.ColorDepth = ColorDepth.Depth32Bit;
            listFriend.SmallImageList = friendImageList;

            listFriend.DrawColumnHeader += ListFriend_DrawColumnHeader;
            listFriend.DrawSubItem += ListFriend_DrawSubItem;
            listFriend.DrawItem += ListFriend_DrawItem;
            SetListViewRowHeight(listFriend, 68); // 64 + padding

            RefreshUserAvatar();
            label1.Text = Emoji.Open_Mouth;
            label2.Text = Emoji.Hushed;
            label3.Text = Emoji.Grimacing;
            label4.Text = Emoji.Neutral_Face;
            label5.Text = Emoji.Sunglasses;
            label6.Text = Emoji.Angry;
        }
        #endregion


        #region 好友列表显示
        private void ListFriend_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawBackground(); // 不显示表头内容
        }

        private void ListFriend_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            Graphics g = e.Graphics;
            Rectangle originalBounds = e.Bounds;
            ListViewItem item = e.Item;

            int avatarSize = 128;
            int padding = 10;

            // 强制行高
            int rowHeight = avatarSize + 4;
            Rectangle newBounds = new Rectangle(originalBounds.Left, originalBounds.Top, originalBounds.Width, rowHeight);

            // 头像绘制区域
            Rectangle imageRect = new Rectangle(newBounds.Left + padding, newBounds.Top + 2, avatarSize, avatarSize);

            // 昵称区域
            Rectangle textRect = new Rectangle(imageRect.Right + padding, newBounds.Top + (avatarSize - 20) / 2, newBounds.Width - avatarSize - 3 * padding, 20);

            if (friendImageList.Images.ContainsKey(item.ImageKey))
            {
                Image avatar = friendImageList.Images[item.ImageKey];
                g.DrawImage(avatar, imageRect);
            }

            using (Font font = new Font("微软雅黑", 10, FontStyle.Bold))
            {
                TextRenderer.DrawText(g, item.Text, font, textRect, Color.RoyalBlue, TextFormatFlags.Left);
            }
        }


        private void RefreshFriendList()
        {
            try
            {
                // 1. 读取最新好友列表
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

                // 2. 更新当前缓存
                friend = newFriendDict;

                // 3. 清空并重建头像列表
                listFriend.View = View.LargeIcon;
                listFriend.LargeImageList = friendImageList;

                if (listFriend.Columns.Count == 0)
                    listFriend.Columns.Add("", listFriend.Width - 5);

                friendImageList.Images.Clear();
                listFriend.Items.Clear();

                // 4. 添加所有好友项
                foreach (var kvp in friend)
                {
                    string uid = kvp.Key;
                    string name = kvp.Value;

                    Image avatar = GetUserAvatar(uid);
                    string imageKey = uid;

                    if (!friendImageList.Images.ContainsKey(imageKey))
                    {
                        Bitmap avatarResized = new Bitmap(avatar, new Size(128, 128));
                        friendImageList.Images.Add(imageKey, avatarResized);

                        // 添加带红点版本
                        Image avatarWithDot = AddRedDotToAvatar(avatarResized);
                        friendImageList.Images.Add(imageKey + "_notify", avatarWithDot);
                    }

                    ListViewItem item = new ListViewItem(name);
                    item.Tag = uid; // 保存真实UID
                    item.ImageKey = uidWithRedDot.Contains(uid) ? imageKey + "_notify" : imageKey;
                    listFriend.Items.Add(item);
                }

                // 5. 添加群聊项
                if (!friendImageList.Images.ContainsKey("group"))
                {
                    try
                    {
                        Image groupAvatar = new Bitmap(Properties.Resources.group_avatar, new Size(128, 128));
                        friendImageList.Images.Add("group", groupAvatar);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("加载群聊头像失败：" + ex.Message);
                        friendImageList.Images.Add("group", Properties.Resources.default_avatar); // fallback
                    }
                }

                ListViewItem groupItem = new ListViewItem("群聊");
                groupItem.ImageKey = "group";
                listFriend.Items.Add(groupItem);
            }
            catch (Exception ex)
            {
                Console.WriteLine("刷新好友列表失败：" + ex.Message);
            }

            if (string.IsNullOrEmpty(chatFriend))
            {
                chatFriend = "000000"; // 默认进入群聊
                ShowChatBox("群聊");
            }

        }

        private Image GetUserAvatar(string uid)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectstring))
                {
                    conn.Open();
                    string sql = "SELECT Avatar FROM userinfo WHERE UID = @uid";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", uid);
                        object result = cmd.ExecuteScalar();

                        if (result != null && result != DBNull.Value)
                        {
                            byte[] bytes = (byte[])result;
                            using (MemoryStream ms = new MemoryStream(bytes))
                            {
                                return Image.FromStream(ms);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"读取用户 {uid} 的头像失败：" + ex.Message);
            }

            return Properties.Resources.default_avatar; // 返回默认头像
        }
        #endregion


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
            Console.WriteLine("[客户端] 接收线程启动");

            while (isRunning)
            {
                try
                {
                    // === 第一步：读取4字节长度头 ===
                    byte[] lengthBytes = new byte[4];
                    int bytesRead = stream.Read(lengthBytes, 0, 4);
                    if (bytesRead != 4)
                    {
                        Console.WriteLine("[客户端] 接收长度失败，连接可能关闭");
                        break;
                    }

                    int messageLength = BitConverter.ToInt32(lengthBytes, 0);
                    if (messageLength <= 0 || messageLength > 10_000_000)
                    {
                        Console.WriteLine($"[客户端] 非法消息长度: {messageLength}");
                        break;
                    }

                    // === 第二步：读取完整消息体 ===
                    byte[] messageBytes = new byte[messageLength];
                    int totalRead = 0;
                    while (totalRead < messageLength)
                    {
                        int read = stream.Read(messageBytes, totalRead, messageLength - totalRead);
                        if (read == 0)
                        {
                            Console.WriteLine("[客户端] 服务器断开连接");
                            break;
                        }
                        totalRead += read;
                    }

                    if (totalRead != messageLength)
                    {
                        Console.WriteLine("[客户端] 消息未读完整");
                        break;
                    }

                    string fullMessage = Encoding.UTF8.GetString(messageBytes);
                    Console.WriteLine($"[客户端] 收到消息：{fullMessage}");
                    // ===第三步：拆解消息格式 ===
                    string pattern = @"^(.*?)\$(.*?)\$(.*?)$";
                    Match match = Regex.Match(fullMessage, pattern);

                    if (!match.Success)
                    {
                        Console.WriteLine("[客户端] 消息格式错误，未匹配正则");
                        continue;
                    }

                    string message = match.Groups[1].Value;
                    string senderUID = match.Groups[2].Value;
                    string receiverUID = match.Groups[3].Value;

                    if (receiverUID == "000000" && senderUID == Myaccount)
                        continue;

                    HandleReceivedMessage(message, senderUID, receiverUID);
                }
                catch (IOException ioEx)
                {
                    Console.WriteLine("[客户端] IO异常：" + ioEx.Message);
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[客户端] 接收异常：" + ex);
                    break;
                }
            }
        }
        #endregion

        #region 新消息提醒：用户头像上显示红点
        private void ShowRedDotOnAvatar(string uid)
        {
            uidWithRedDot.Add(uid); // 记录红点状态

            foreach (ListViewItem item in listFriend.Items)
            {
                if ((string)item.Tag == uid)
                {
                    if (friendImageList.Images.ContainsKey(uid + "_notify"))
                        item.ImageKey = uid + "_notify";
                    break;
                }
            }
        }

        private void ClearRedDotFromAvatar(string uid)
        {
            uidWithRedDot.Remove(uid); // 移除红点状态

            foreach (ListViewItem item in listFriend.Items)
            {
                if ((string)item.Tag == uid)
                {
                    item.ImageKey = uid;
                    break;
                }
            }
        }


        #endregion

        #region 收取消息
        private void HandleReceivedMessage(string message, string senderUID, string receiverUID)
        {
            string displayName = (receiverUID == "000000")
                ? "群聊"
                : (senderUID == Myaccount
                    ? (friend.ContainsKey(receiverUID) ? friend[receiverUID] : receiverUID)
                    : (friend.ContainsKey(senderUID) ? friend[senderUID] : senderUID));

            this.Invoke(new Action(() =>
            {
                // 创建聊天框面板（如果不存在）
                Control[] chatBoxes = panelChat.Controls.Find(displayName, false);
                if (chatBoxes.Length == 0)
                {
                    ShowChatBox(displayName);
                }

                // === 是否为当前正在聊天对象，如果不是，头像加红点 ===
                bool isGroup = receiverUID == "000000";
                // 修复为只使用 UID
                string senderUIDForRedDot = isGroup ? "000000" : (senderUID == Myaccount ? receiverUID : senderUID);

                if (chatFriend != senderUIDForRedDot)
                {
                    ShowRedDotOnAvatar(senderUIDForRedDot);
                }


                // === 文件消息处理 ===
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

        #endregion


        #region 发消息事件
        private void buttonSend_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textSend.Text))
                return;

            string message = textSend.Text.Trim();
            textSend.Text = "";

            try
            {
                // 获取 receiverUID
                string receiverUID = chatFriend == "群聊" ? "000000" : chatFriend;

                // 构造消息格式：receiverUID$message$Myaccount
                string sendMessage = receiverUID + "$" + message + "$" + Myaccount;
                string filteredMessage = FilterMessageBody(sendMessage);
                if (string.IsNullOrWhiteSpace(filteredMessage))
                {
                    Console.WriteLine("消息为空或被过滤为无效，已拦截");
                    return;
                }
                // 转为 UTF8 字节
                byte[] messageBody = Encoding.UTF8.GetBytes(sendMessage);
             

                // 构造前4字节长度头
                byte[] lengthBytes = BitConverter.GetBytes(messageBody.Length);

                // 拼接完整消息 = [长度头] + [内容体]
                byte[] fullMessage = new byte[4 + messageBody.Length];
                Buffer.BlockCopy(lengthBytes, 0, fullMessage, 0, 4);
                Buffer.BlockCopy(messageBody, 0, fullMessage, 4, messageBody.Length);

                if (stream != null && stream.CanWrite)
                {
                    stream.Write(fullMessage, 0, fullMessage.Length);
                }
                else
                {
                    MessageBox.Show("发送失败：网络连接已断开。");
                    return;
                }

                // 本地显示
                string displayName = (receiverUID == "000000")
                    ? "群聊"
                    : (friend.ContainsKey(receiverUID) ? friend[receiverUID] : receiverUID);

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
                string uid = friendName == "群聊" ? "群聊" : friend.FirstOrDefault(x => x.Value == friendName).Key;
                // 清除红点
                ClearRedDotFromAvatar(uid);
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
                chatBox.HorizontalScroll.Maximum = 0;
                chatBox.HorizontalScroll.Visible = false;
                chatBox.AutoScroll = true;
                //  新建完后，加载聊天历史
                LoadHistoryMessages(friendName);
            }
        }



        #endregion

        #region 搜索用户添加好友
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
        #endregion

        #region 发送文件
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
                string base64Data = Convert.ToBase64String(fileData);

                // 构造协议字符串
                string sendMessage = chatFriend + "$" + "FILE:" + fileName + "#" + fileData.Length + "#" + base64Data + "$" + Myaccount;
                byte[] messageBody = Encoding.UTF8.GetBytes(sendMessage);
                byte[] lengthBytes = BitConverter.GetBytes(messageBody.Length);

                byte[] fullMessage = new byte[4 + messageBody.Length];
                Buffer.BlockCopy(lengthBytes, 0, fullMessage, 0, 4);
                Buffer.BlockCopy(messageBody, 0, fullMessage, 4, messageBody.Length);

                if (stream != null && stream.CanWrite)
                {
                    stream.Write(fullMessage, 0, fullMessage.Length);
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
        #endregion

        #region 过滤消息
        private string FilterMessageBody(string messageBody)
        {
            // 清除 HTML
            messageBody = Regex.Replace(messageBody, "<.*?>", string.Empty);

            // 替换敏感词
            string[] badWords = { "傻瓜", "废物", "蠢货" };
            foreach (var word in badWords)
            {
                messageBody = Regex.Replace(messageBody, word, "**");
            }

            // 限制长度
            if (messageBody.Length > 300)
            {
                messageBody = messageBody.Substring(0, 300) + " ...";
            }

            return messageBody.Trim();
        }
        #endregion

        #region 保存配置
        private void SaveClientConfig(string uid, string password)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                XmlElement root = doc.CreateElement("ClientConfig");

                XmlElement uidElem = doc.CreateElement("UID");
                uidElem.InnerText = uid;

                XmlElement pwdElem = doc.CreateElement("Password");
                pwdElem.InnerText = password;

                XmlElement dbElem = doc.CreateElement("Database");

                XmlElement hostElem = doc.CreateElement("Host");
                hostElem.InnerText = "localhost";

                XmlElement portElem = doc.CreateElement("Port");
                portElem.InnerText = "3306";

                XmlElement nameElem = doc.CreateElement("Name");
                nameElem.InnerText = "wechatting";

                XmlElement userElem = doc.CreateElement("User");
                userElem.InnerText = "root";

                XmlElement dbPwdElem = doc.CreateElement("DbPassword");
                dbPwdElem.InnerText = "123456";

                dbElem.AppendChild(hostElem);
                dbElem.AppendChild(portElem);
                dbElem.AppendChild(nameElem);
                dbElem.AppendChild(userElem);
                dbElem.AppendChild(dbPwdElem);

                root.AppendChild(uidElem);
                root.AppendChild(pwdElem);
                root.AppendChild(dbElem);

                doc.AppendChild(root);
                doc.Save("client_config.xml");
                // 更新内存中的值
                DbConfig.UID = uid;
                DbConfig.Password = password;
                MessageBox.Show("配置已保存");
            }
            catch (Exception ex)
            {
                MessageBox.Show("保存失败：" + ex.Message);
            }
        }
        #endregion

        #region 加载头像
        private void button4_Click(object sender, EventArgs e)
        {
            // 弹出文件选择框
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "请选择头像图片";
            dialog.Filter = "图片文件 (*.jpg;*.png;*.jpeg)|*.jpg;*.png;*.jpeg";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string selectedPath = dialog.FileName;

                // 显示到 PictureBox（预览效果）
                pictureBoxAvatar.Image = Image.FromFile(selectedPath);

                // 上传保存到数据库
                SaveAvatarToDatabase(Myaccount, selectedPath);  // Myaccount 是当前登录 UID
            }
        }
        private void RefreshUserAvatar()
        {
            Image avatar = null;

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectstring))
                {
                    conn.Open();
                    string sql = "SELECT Avatar FROM userinfo WHERE UID = @uid";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", Myaccount);
                        object result = cmd.ExecuteScalar();

                        if (result != null && result != DBNull.Value)
                        {
                            byte[] bytes = (byte[])result;
                            //  MessageBox.Show($"数据库中头像大小：{bytes.Length} 字节", "调试");

                            using (MemoryStream ms = new MemoryStream(bytes))
                            {
                                avatar = Image.FromStream(ms);
                            }
                        }
                        else
                        {
                            //MessageBox.Show("数据库中未找到头像", "调试");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("读取头像失败：" + ex.Message, "异常");
            }

            if (avatar == null)
            {
                try
                {
                    avatar = Properties.Resources.default_avatar;
                    //MessageBox.Show("使用内置默认头像（Resources.resx）", "调试");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("加载默认头像异常：" + ex.Message, "异常");
                }
            }

            pictureBoxAvatar.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxAvatar.Image = avatar;
        }
        private void ListFriend_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            // 不画背景，让 DrawSubItem 来画
            e.DrawBackground();
        }
        private void SetListViewRowHeight(ListView lv, int height)
        {
            // 创建一个假的 ImageList 设置高度
            ImageList imgList = new ImageList();
            imgList.ImageSize = new Size(1, height);
            lv.SmallImageList = imgList;
        }
        private Image AddRedDotToAvatar(Image original)
        {
            Bitmap bmp = new Bitmap(original);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                int dotSize = 16;
                int padding = 4;

                int x = bmp.Width - dotSize - padding;
                int y = padding;

                using (Brush redBrush = new SolidBrush(Color.Red))
                {
                    g.FillEllipse(redBrush, x, y, dotSize, dotSize);
                }
            }
            return bmp;
        }
        private void SaveAvatarToDatabase(string uid, string imagePath)
        {
            try
            {
                byte[] avatarBytes = File.ReadAllBytes(imagePath);

                using (MySqlConnection conn = new MySqlConnection(connectstring))
                {
                    conn.Open();
                    string sql = "UPDATE userinfo SET Avatar = @avatar WHERE UID = @uid";

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@avatar", avatarBytes);
                        cmd.Parameters.AddWithValue("@uid", uid);
                        cmd.ExecuteNonQuery();
                    }
                }
                RefreshUserAvatar();
                MessageBox.Show("头像上传成功！");
            }
            catch (Exception ex)
            {
                MessageBox.Show("头像上传失败：" + ex.Message);
            }
        }
#endregion

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

        private void listMessage_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void labelAdd_Click(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            SaveClientConfig(Myaccount,MyPassword);
        }

       

        private void pictureBoxAvatar_Click(object sender, EventArgs e)
        {

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
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            isRunning = false;
            stream?.Close();
            client?.Close();
            client?.Dispose();
            Application.Exit();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            LogIn loginForm = new LogIn();
            loginForm.Show();     // 显示登录窗口
            this.Hide();
        }
    }
}