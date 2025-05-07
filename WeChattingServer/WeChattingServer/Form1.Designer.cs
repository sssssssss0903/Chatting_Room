
namespace WeChattingServer
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
            this.label1 = new System.Windows.Forms.Label();
            this.textServerPort = new System.Windows.Forms.TextBox();
            this.labelServerPort = new System.Windows.Forms.Label();
            this.buttonListen = new System.Windows.Forms.Button();
            this.listServerMessage = new System.Windows.Forms.ListBox();
            this.listOnlineUsers = new System.Windows.Forms.ListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(243, 14);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(730, 24);
            this.label1.TabIndex = 0;
            this.label1.Text = "欢迎来到ChattingRoom的服务端，在这里您能监视到所有用户的聊天";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // textServerPort
            // 
            this.textServerPort.Location = new System.Drawing.Point(519, 58);
            this.textServerPort.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.textServerPort.Name = "textServerPort";
            this.textServerPort.Size = new System.Drawing.Size(148, 35);
            this.textServerPort.TabIndex = 1;
            // 
            // labelServerPort
            // 
            this.labelServerPort.AutoSize = true;
            this.labelServerPort.Location = new System.Drawing.Point(420, 62);
            this.labelServerPort.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelServerPort.Name = "labelServerPort";
            this.labelServerPort.Size = new System.Drawing.Size(58, 24);
            this.labelServerPort.TabIndex = 2;
            this.labelServerPort.Text = "Port";
            // 
            // buttonListen
            // 
            this.buttonListen.Location = new System.Drawing.Point(724, 50);
            this.buttonListen.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.buttonListen.Name = "buttonListen";
            this.buttonListen.Size = new System.Drawing.Size(182, 47);
            this.buttonListen.TabIndex = 3;
            this.buttonListen.Text = "开始监听";
            this.buttonListen.UseVisualStyleBackColor = true;
            this.buttonListen.Click += new System.EventHandler(this.buttonListen_Click);
            // 
            // listServerMessage
            // 
            this.listServerMessage.FormattingEnabled = true;
            this.listServerMessage.ItemHeight = 24;
            this.listServerMessage.Location = new System.Drawing.Point(34, 119);
            this.listServerMessage.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.listServerMessage.Name = "listServerMessage";
            this.listServerMessage.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.listServerMessage.Size = new System.Drawing.Size(847, 532);
            this.listServerMessage.TabIndex = 4;
            // 
            // listOnlineUsers
            // 
            this.listOnlineUsers.FormattingEnabled = true;
            this.listOnlineUsers.ItemHeight = 24;
            this.listOnlineUsers.Location = new System.Drawing.Point(927, 167);
            this.listOnlineUsers.Name = "listOnlineUsers";
            this.listOnlineUsers.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.listOnlineUsers.Size = new System.Drawing.Size(249, 484);
            this.listOnlineUsers.TabIndex = 5;
            this.listOnlineUsers.SelectedIndexChanged += new System.EventHandler(this.listOnlineUsers_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(966, 119);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(154, 24);
            this.label2.TabIndex = 6;
            this.label2.Text = "在线用户列表";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 720);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.listOnlineUsers);
            this.Controls.Add(this.listServerMessage);
            this.Controls.Add(this.buttonListen);
            this.Controls.Add(this.labelServerPort);
            this.Controls.Add(this.textServerPort);
            this.Controls.Add(this.label1);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textServerPort;
        private System.Windows.Forms.Label labelServerPort;
        private System.Windows.Forms.Button buttonListen;
        private System.Windows.Forms.ListBox listServerMessage;
        private System.Windows.Forms.ListBox listOnlineUsers;
        private System.Windows.Forms.Label label2;
    }
}

