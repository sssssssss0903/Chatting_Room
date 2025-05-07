using System.Drawing;
namespace WeChattingClient
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.textSend = new System.Windows.Forms.TextBox();
            this.buttonSend = new System.Windows.Forms.Button();
            this.listFriend = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.panelChat = new System.Windows.Forms.Panel();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.textAdd = new System.Windows.Forms.TextBox();
            this.labelAdd = new System.Windows.Forms.Label();
            this.buttonSureAdd = new System.Windows.Forms.Button();
            this.listAdd = new System.Windows.Forms.ListBox();
            this.buttonReturn = new System.Windows.Forms.Button();
            this.buttonAddFriend = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.pictureBoxAvatar = new System.Windows.Forms.PictureBox();
            this.label7 = new System.Windows.Forms.Label();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxAvatar)).BeginInit();
            this.SuspendLayout();
            // 
            // textSend
            // 
            this.textSend.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textSend.Location = new System.Drawing.Point(256, 799);
            this.textSend.Margin = new System.Windows.Forms.Padding(4);
            this.textSend.Name = "textSend";
            this.textSend.Size = new System.Drawing.Size(580, 50);
            this.textSend.TabIndex = 0;
            this.textSend.TextChanged += new System.EventHandler(this.textSend_TextChanged);
            // 
            // buttonSend
            // 
            this.buttonSend.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.buttonSend.Location = new System.Drawing.Point(867, 795);
            this.buttonSend.Margin = new System.Windows.Forms.Padding(4);
            this.buttonSend.Name = "buttonSend";
            this.buttonSend.Size = new System.Drawing.Size(168, 56);
            this.buttonSend.TabIndex = 1;
            this.buttonSend.Text = "发送";
            this.buttonSend.UseVisualStyleBackColor = true;
            this.buttonSend.Click += new System.EventHandler(this.buttonSend_Click);
            // 
            // listFriend
            // 
            this.listFriend.BackColor = System.Drawing.Color.DarkSeaGreen;
            this.listFriend.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.listFriend.Dock = System.Windows.Forms.DockStyle.Left;
            this.listFriend.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.listFriend.ForeColor = System.Drawing.SystemColors.MenuHighlight;
            this.listFriend.HideSelection = false;
            this.listFriend.Location = new System.Drawing.Point(0, 0);
            this.listFriend.Margin = new System.Windows.Forms.Padding(4);
            this.listFriend.Name = "listFriend";
            this.listFriend.Size = new System.Drawing.Size(236, 1016);
            this.listFriend.TabIndex = 10;
            this.listFriend.View = System.Windows.Forms.View.Details;
            this.listFriend.SelectedIndexChanged += new System.EventHandler(this.listFriend_SelectedIndexChanged);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "好友列表";
            this.columnHeader1.Width = 120;
            // 
            // panelChat
            // 
            this.panelChat.AutoScroll = true;
            this.panelChat.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panelChat.BackgroundImage")));
            this.panelChat.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.panelChat.Location = new System.Drawing.Point(236, 0);
            this.panelChat.Margin = new System.Windows.Forms.Padding(4);
            this.panelChat.Name = "panelChat";
            this.panelChat.Size = new System.Drawing.Size(963, 773);
            this.panelChat.TabIndex = 11;
            this.panelChat.Paint += new System.Windows.Forms.PaintEventHandler(this.panelChat_Paint);
            // 
            // buttonAdd
            // 
            this.buttonAdd.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.buttonAdd.Location = new System.Drawing.Point(1058, 788);
            this.buttonAdd.Margin = new System.Windows.Forms.Padding(4);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(178, 61);
            this.buttonAdd.TabIndex = 12;
            this.buttonAdd.Text = "添加好友";
            this.buttonAdd.UseVisualStyleBackColor = true;
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);

            // 
            // textAdd
            // 
            this.textAdd.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textAdd.Location = new System.Drawing.Point(1364, 310);
            this.textAdd.Margin = new System.Windows.Forms.Padding(4);
            this.textAdd.Name = "textAdd";
            this.textAdd.Size = new System.Drawing.Size(169, 50);
            this.textAdd.TabIndex = 13;
            this.textAdd.Visible = false;
            this.textAdd.TextChanged += new System.EventHandler(this.textAdd_TextChanged);
            // 
            // labelAdd
            // 
            this.labelAdd.AutoSize = true;
            this.labelAdd.BackColor = System.Drawing.Color.DarkSeaGreen;
            this.labelAdd.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labelAdd.Location = new System.Drawing.Point(1207, 319);
            this.labelAdd.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelAdd.Name = "labelAdd";
            this.labelAdd.Size = new System.Drawing.Size(139, 41);
            this.labelAdd.TabIndex = 14;
            this.labelAdd.Text = "输入UID";
            this.labelAdd.Visible = false;
            this.labelAdd.Click += new System.EventHandler(this.labelAdd_Click);
            // 
            // buttonSureAdd
            // 
            this.buttonSureAdd.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.buttonSureAdd.Location = new System.Drawing.Point(1204, 382);
            this.buttonSureAdd.Margin = new System.Windows.Forms.Padding(4);
            this.buttonSureAdd.Name = "buttonSureAdd";
            this.buttonSureAdd.Size = new System.Drawing.Size(116, 60);
            this.buttonSureAdd.TabIndex = 15;
            this.buttonSureAdd.Text = "搜索";
            this.buttonSureAdd.UseVisualStyleBackColor = true;
            this.buttonSureAdd.Visible = false;
            this.buttonSureAdd.Click += new System.EventHandler(this.buttonSureAdd_Click);
            // 
            // listAdd
            // 
            this.listAdd.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.listAdd.FormattingEnabled = true;
            this.listAdd.ItemHeight = 41;
            this.listAdd.Location = new System.Drawing.Point(1207, 450);
            this.listAdd.Margin = new System.Windows.Forms.Padding(4);
            this.listAdd.Name = "listAdd";
            this.listAdd.Size = new System.Drawing.Size(295, 250);
            this.listAdd.TabIndex = 16;
            this.listAdd.Visible = false;
            // 
            // buttonReturn
            // 
            this.buttonReturn.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.buttonReturn.Location = new System.Drawing.Point(1342, 382);
            this.buttonReturn.Margin = new System.Windows.Forms.Padding(4);
            this.buttonReturn.Name = "buttonReturn";
            this.buttonReturn.Size = new System.Drawing.Size(108, 60);
            this.buttonReturn.TabIndex = 17;
            this.buttonReturn.Text = "返回";
            this.buttonReturn.UseVisualStyleBackColor = true;
            this.buttonReturn.Visible = false;
            this.buttonReturn.Click += new System.EventHandler(this.buttonReturn_Click);
            // 
            // buttonAddFriend
            // 
            this.buttonAddFriend.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.buttonAddFriend.Location = new System.Drawing.Point(1058, 788);
            this.buttonAddFriend.Margin = new System.Windows.Forms.Padding(4);
            this.buttonAddFriend.Name = "buttonAddFriend";
            this.buttonAddFriend.Size = new System.Drawing.Size(178, 61);
            this.buttonAddFriend.TabIndex = 18;
            this.buttonAddFriend.Text = "确认添加";
            this.buttonAddFriend.UseVisualStyleBackColor = true;
            this.buttonAddFriend.Visible = false;
            this.buttonAddFriend.Click += new System.EventHandler(this.buttonAddFriend_Click);
            // 
            // textBox1
            // 
            this.textBox1.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox1.Location = new System.Drawing.Point(256, 873);
            this.textBox1.Margin = new System.Windows.Forms.Padding(4);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(580, 50);
            this.textBox1.TabIndex = 19;
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button1.Location = new System.Drawing.Point(867, 873);
            this.button1.Margin = new System.Windows.Forms.Padding(4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(168, 56);
            this.button1.TabIndex = 20;
            this.button1.Text = "选择文件";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button2.Location = new System.Drawing.Point(1058, 873);
            this.button2.Margin = new System.Windows.Forms.Padding(4);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(178, 56);
            this.button2.TabIndex = 21;
            this.button2.Text = "发送文件";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.White;
            this.label1.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(1250, 791);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(111, 33);
            this.label1.TabIndex = 22;
            this.label1.Text = "label1";
            this.label1.Click += new System.EventHandler(this.label1_Click_1);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.White;
            this.label2.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(1358, 791);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(111, 33);
            this.label2.TabIndex = 23;
            this.label2.Text = "label2";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.White;
            this.label3.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label3.Location = new System.Drawing.Point(1250, 844);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(111, 33);
            this.label3.TabIndex = 24;
            this.label3.Text = "label3";
            this.label3.Click += new System.EventHandler(this.label3_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.BackColor = System.Drawing.Color.White;
            this.label4.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label4.Location = new System.Drawing.Point(1358, 844);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(111, 33);
            this.label4.TabIndex = 25;
            this.label4.Text = "label4";
            this.label4.Click += new System.EventHandler(this.label4_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.BackColor = System.Drawing.Color.White;
            this.label5.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label5.Location = new System.Drawing.Point(1250, 896);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(111, 33);
            this.label5.TabIndex = 26;
            this.label5.Text = "label5";
            this.label5.Click += new System.EventHandler(this.label5_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.BackColor = System.Drawing.Color.White;
            this.label6.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label6.Location = new System.Drawing.Point(1358, 896);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(111, 33);
            this.label6.TabIndex = 27;
            this.label6.Text = "label6";
            this.label6.Click += new System.EventHandler(this.label6_Click);
            // 
            // pictureBoxAvatar
            // 
            this.pictureBoxAvatar.BackColor = System.Drawing.Color.Transparent;
            this.pictureBoxAvatar.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.pictureBoxAvatar.Image = global::WeChattingClient.Properties.Resources.default_avatar;
            this.pictureBoxAvatar.Location = new System.Drawing.Point(1204, 0);
            this.pictureBoxAvatar.Name = "pictureBoxAvatar";
            this.pictureBoxAvatar.Size = new System.Drawing.Size(180, 195);
            this.pictureBoxAvatar.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxAvatar.TabIndex = 28;
            this.pictureBoxAvatar.TabStop = false;
            this.pictureBoxAvatar.Click += new System.EventHandler(this.pictureBoxAvatar_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.BackColor = System.Drawing.Color.Transparent;
            this.label7.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.label7.Location = new System.Drawing.Point(1406, 75);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(82, 31);
            this.label7.TabIndex = 29;
            this.label7.Text = "label7";
            this.label7.Click += new System.EventHandler(this.label7_Click);
            // 
            // button3
            // 
            this.button3.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.button3.Location = new System.Drawing.Point(1390, 140);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(169, 55);
            this.button3.TabIndex = 30;
            this.button3.Text = "记住账号";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button4
            // 
            this.button4.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.button4.Location = new System.Drawing.Point(1204, 201);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(169, 57);
            this.button4.TabIndex = 31;
            this.button4.Text = "修改头像";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button5
            // 
            this.button5.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.button5.Location = new System.Drawing.Point(1390, 201);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(168, 56);
            this.button5.TabIndex = 32;
            this.button5.Text = "退出登录";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.ClientSize = new System.Drawing.Size(1654, 1016);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.pictureBoxAvatar);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.buttonAddFriend);
            this.Controls.Add(this.buttonReturn);
            this.Controls.Add(this.listAdd);
            this.Controls.Add(this.buttonSureAdd);
            this.Controls.Add(this.labelAdd);
            this.Controls.Add(this.textAdd);
            this.Controls.Add(this.buttonAdd);
            this.Controls.Add(this.panelChat);
            this.Controls.Add(this.listFriend);
            this.Controls.Add(this.buttonSend);
            this.Controls.Add(this.textSend);
            this.DoubleBuffered = true;
            this.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxAvatar)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textSend;
        private System.Windows.Forms.Button buttonSend;
    
   
        private System.Windows.Forms.ListView listFriend;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.Panel panelChat;
        private System.Windows.Forms.Button buttonAdd;
        private System.Windows.Forms.TextBox textAdd;
        private System.Windows.Forms.Label labelAdd;
        private System.Windows.Forms.Button buttonSureAdd;
        private System.Windows.Forms.ListBox listAdd;
        private System.Windows.Forms.Button buttonReturn;
        private System.Windows.Forms.Button buttonAddFriend;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.PictureBox pictureBoxAvatar;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
    }
}

