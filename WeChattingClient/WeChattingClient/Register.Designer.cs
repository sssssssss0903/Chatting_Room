
namespace WeChattingClient
{
    partial class Register
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.buttonRegister = new System.Windows.Forms.Button();
            this.textName = new System.Windows.Forms.TextBox();
            this.textPassword1 = new System.Windows.Forms.TextBox();
            this.textPassword2 = new System.Windows.Forms.TextBox();
            this.textAccount = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // buttonRegister
            // 
            this.buttonRegister.BackColor = System.Drawing.Color.DarkSeaGreen;
            this.buttonRegister.Font = new System.Drawing.Font("Comic Sans MS", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonRegister.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.buttonRegister.Location = new System.Drawing.Point(45, 260);
            this.buttonRegister.Margin = new System.Windows.Forms.Padding(2);
            this.buttonRegister.Name = "buttonRegister";
            this.buttonRegister.Size = new System.Drawing.Size(100, 41);
            this.buttonRegister.TabIndex = 0;
            this.buttonRegister.Text = "Register";
            this.buttonRegister.UseVisualStyleBackColor = false;
            this.buttonRegister.Click += new System.EventHandler(this.buttonRegister_Click);
            // 
            // textName
            // 
            this.textName.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textName.Location = new System.Drawing.Point(153, 102);
            this.textName.Margin = new System.Windows.Forms.Padding(2);
            this.textName.Name = "textName";
            this.textName.Size = new System.Drawing.Size(87, 29);
            this.textName.TabIndex = 1;
            // 
            // textPassword1
            // 
            this.textPassword1.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textPassword1.Location = new System.Drawing.Point(153, 150);
            this.textPassword1.Margin = new System.Windows.Forms.Padding(2);
            this.textPassword1.Name = "textPassword1";
            this.textPassword1.Size = new System.Drawing.Size(87, 29);
            this.textPassword1.TabIndex = 3;
            this.textPassword1.TextChanged += new System.EventHandler(this.textPassword1_TextChanged);
            // 
            // textPassword2
            // 
            this.textPassword2.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textPassword2.Location = new System.Drawing.Point(153, 199);
            this.textPassword2.Margin = new System.Windows.Forms.Padding(2);
            this.textPassword2.Name = "textPassword2";
            this.textPassword2.Size = new System.Drawing.Size(87, 29);
            this.textPassword2.TabIndex = 4;
            this.textPassword2.TextChanged += new System.EventHandler(this.textPassword2_TextChanged);
            // 
            // textAccount
            // 
            this.textAccount.BackColor = System.Drawing.SystemColors.Window;
            this.textAccount.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textAccount.Location = new System.Drawing.Point(153, 57);
            this.textAccount.Margin = new System.Windows.Forms.Padding(2);
            this.textAccount.Name = "textAccount";
            this.textAccount.ReadOnly = true;
            this.textAccount.Size = new System.Drawing.Size(87, 29);
            this.textAccount.TabIndex = 7;
            // 
            // Register
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::WeChattingClient.Properties.Resources.zhuce;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(600, 360);
            this.Controls.Add(this.textAccount);
            this.Controls.Add(this.textPassword2);
            this.Controls.Add(this.textPassword1);
            this.Controls.Add(this.textName);
            this.Controls.Add(this.buttonRegister);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Register";
            this.Text = "Register";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonRegister;
        private System.Windows.Forms.TextBox textName;
        private System.Windows.Forms.TextBox textPassword1;
        private System.Windows.Forms.TextBox textPassword2;
        private System.Windows.Forms.TextBox textAccount;
    }
}