
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
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(162, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(447, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "欢迎来到WeChatting的服务端，在这里您能监视到所有用户的聊天";
            // 
            // textServerPort
            // 
            this.textServerPort.Location = new System.Drawing.Point(346, 36);
            this.textServerPort.Name = "textServerPort";
            this.textServerPort.Size = new System.Drawing.Size(100, 25);
            this.textServerPort.TabIndex = 1;
            // 
            // labelServerPort
            // 
            this.labelServerPort.AutoSize = true;
            this.labelServerPort.Location = new System.Drawing.Point(280, 39);
            this.labelServerPort.Name = "labelServerPort";
            this.labelServerPort.Size = new System.Drawing.Size(39, 15);
            this.labelServerPort.TabIndex = 2;
            this.labelServerPort.Text = "Port";
            // 
            // buttonListen
            // 
            this.buttonListen.Location = new System.Drawing.Point(557, 36);
            this.buttonListen.Name = "buttonListen";
            this.buttonListen.Size = new System.Drawing.Size(75, 23);
            this.buttonListen.TabIndex = 3;
            this.buttonListen.Text = "开始监听";
            this.buttonListen.UseVisualStyleBackColor = true;
            this.buttonListen.Click += new System.EventHandler(this.buttonListen_Click);
            // 
            // listServerMessage
            // 
            this.listServerMessage.FormattingEnabled = true;
            this.listServerMessage.ItemHeight = 15;
            this.listServerMessage.Location = new System.Drawing.Point(31, 114);
            this.listServerMessage.Name = "listServerMessage";
            this.listServerMessage.Size = new System.Drawing.Size(741, 274);
            this.listServerMessage.TabIndex = 4;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.listServerMessage);
            this.Controls.Add(this.buttonListen);
            this.Controls.Add(this.labelServerPort);
            this.Controls.Add(this.textServerPort);
            this.Controls.Add(this.label1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textServerPort;
        private System.Windows.Forms.Label labelServerPort;
        private System.Windows.Forms.Button buttonListen;
        private System.Windows.Forms.ListBox listServerMessage;
    }
}

