﻿
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
            this.textSend = new System.Windows.Forms.TextBox();
            this.buttonSend = new System.Windows.Forms.Button();
            this.listMessage = new System.Windows.Forms.ListBox();
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
            this.SuspendLayout();
            // 
            // textSend
            // 
            this.textSend.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textSend.Location = new System.Drawing.Point(88, 378);
            this.textSend.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textSend.Name = "textSend";
            this.textSend.Size = new System.Drawing.Size(306, 29);
            this.textSend.TabIndex = 0;
            this.textSend.TextChanged += new System.EventHandler(this.textSend_TextChanged);
            // 
            // buttonSend
            // 
            this.buttonSend.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.buttonSend.Location = new System.Drawing.Point(397, 377);
            this.buttonSend.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.buttonSend.Name = "buttonSend";
            this.buttonSend.Size = new System.Drawing.Size(76, 28);
            this.buttonSend.TabIndex = 1;
            this.buttonSend.Text = "发送";
            this.buttonSend.UseVisualStyleBackColor = true;
            this.buttonSend.Click += new System.EventHandler(this.buttonSend_Click);
            // 
            // listMessage
            // 
            this.listMessage.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.listMessage.BackColor = System.Drawing.Color.DarkSeaGreen;
            this.listMessage.Font = new System.Drawing.Font("Comic Sans MS", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listMessage.FormattingEnabled = true;
            this.listMessage.ItemHeight = 23;
            this.listMessage.Location = new System.Drawing.Point(480, 23);
            this.listMessage.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.listMessage.Name = "listMessage";
            this.listMessage.Size = new System.Drawing.Size(156, 27);
            this.listMessage.TabIndex = 3;
            // 
            // listFriend
            // 
            this.listFriend.AllowDrop = true;
            this.listFriend.BackColor = System.Drawing.Color.DarkSeaGreen;
            this.listFriend.BackgroundImageTiled = true;
            this.listFriend.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.listFriend.Dock = System.Windows.Forms.DockStyle.Left;
            this.listFriend.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listFriend.ForeColor = System.Drawing.SystemColors.MenuHighlight;
            this.listFriend.HideSelection = false;
            this.listFriend.Location = new System.Drawing.Point(0, 0);
            this.listFriend.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.listFriend.Name = "listFriend";
            this.listFriend.Size = new System.Drawing.Size(85, 410);
            this.listFriend.TabIndex = 10;
            this.listFriend.UseCompatibleStateImageBehavior = false;
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
            this.panelChat.BackgroundImage = global::WeChattingClient.Properties.Resources.微信图片_20230530105952_1__1_;
            this.panelChat.Location = new System.Drawing.Point(89, 0);
            this.panelChat.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.panelChat.Name = "panelChat";
            this.panelChat.Size = new System.Drawing.Size(383, 373);
            this.panelChat.TabIndex = 11;
            this.panelChat.Paint += new System.Windows.Forms.PaintEventHandler(this.panelChat_Paint);
            // 
            // buttonAdd
            // 
            this.buttonAdd.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.buttonAdd.Location = new System.Drawing.Point(509, 378);
            this.buttonAdd.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(84, 30);
            this.buttonAdd.TabIndex = 12;
            this.buttonAdd.Text = "添加好友";
            this.buttonAdd.UseVisualStyleBackColor = true;
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
            // 
            // textAdd
            // 
            this.textAdd.Location = new System.Drawing.Point(551, 92);
            this.textAdd.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textAdd.Name = "textAdd";
            this.textAdd.Size = new System.Drawing.Size(76, 21);
            this.textAdd.TabIndex = 13;
            this.textAdd.Visible = false;
            this.textAdd.TextChanged += new System.EventHandler(this.textAdd_TextChanged);
            // 
            // labelAdd
            // 
            this.labelAdd.AutoSize = true;
            this.labelAdd.BackColor = System.Drawing.Color.DarkSeaGreen;
            this.labelAdd.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labelAdd.Location = new System.Drawing.Point(476, 92);
            this.labelAdd.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelAdd.Name = "labelAdd";
            this.labelAdd.Size = new System.Drawing.Size(71, 21);
            this.labelAdd.TabIndex = 14;
            this.labelAdd.Text = "输入UID";
            this.labelAdd.Visible = false;
            // 
            // buttonSureAdd
            // 
            this.buttonSureAdd.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.buttonSureAdd.Location = new System.Drawing.Point(478, 117);
            this.buttonSureAdd.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.buttonSureAdd.Name = "buttonSureAdd";
            this.buttonSureAdd.Size = new System.Drawing.Size(58, 30);
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
            this.listAdd.ItemHeight = 21;
            this.listAdd.Location = new System.Drawing.Point(478, 160);
            this.listAdd.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.listAdd.Name = "listAdd";
            this.listAdd.Size = new System.Drawing.Size(153, 172);
            this.listAdd.TabIndex = 16;
            this.listAdd.Visible = false;
            // 
            // buttonReturn
            // 
            this.buttonReturn.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.buttonReturn.Location = new System.Drawing.Point(559, 117);
            this.buttonReturn.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.buttonReturn.Name = "buttonReturn";
            this.buttonReturn.Size = new System.Drawing.Size(54, 30);
            this.buttonReturn.TabIndex = 17;
            this.buttonReturn.Text = "返回";
            this.buttonReturn.UseVisualStyleBackColor = true;
            this.buttonReturn.Visible = false;
            this.buttonReturn.Click += new System.EventHandler(this.buttonReturn_Click);
            // 
            // buttonAddFriend
            // 
            this.buttonAddFriend.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.buttonAddFriend.Location = new System.Drawing.Point(509, 377);
            this.buttonAddFriend.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.buttonAddFriend.Name = "buttonAddFriend";
            this.buttonAddFriend.Size = new System.Drawing.Size(84, 27);
            this.buttonAddFriend.TabIndex = 18;
            this.buttonAddFriend.Text = "确认添加";
            this.buttonAddFriend.UseVisualStyleBackColor = true;
            this.buttonAddFriend.Visible = false;
            this.buttonAddFriend.Click += new System.EventHandler(this.buttonAddFriend_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.OliveDrab;
            this.BackgroundImage = global::WeChattingClient.Properties.Resources.微信图片_20230530110704;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(638, 410);
            this.Controls.Add(this.listMessage);
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
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textSend;
        private System.Windows.Forms.Button buttonSend;
        private System.Windows.Forms.ListBox listMessage;
   
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
    }
}

