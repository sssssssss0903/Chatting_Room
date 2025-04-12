
namespace WeChattingClient
{
    partial class LogIn
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
            this.buttonLogIn = new System.Windows.Forms.Button();
            this.textInputAccount = new System.Windows.Forms.TextBox();
            this.textInputPassword = new System.Windows.Forms.TextBox();
            this.buttonRegister = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonLogIn
            // 
            this.buttonLogIn.BackColor = System.Drawing.Color.DarkSeaGreen;
            this.buttonLogIn.Font = new System.Drawing.Font("黑体", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.buttonLogIn.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.buttonLogIn.Location = new System.Drawing.Point(261, 396);
            this.buttonLogIn.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonLogIn.Name = "buttonLogIn";
            this.buttonLogIn.Size = new System.Drawing.Size(103, 46);
            this.buttonLogIn.TabIndex = 0;
            this.buttonLogIn.Text = "登录";
            this.buttonLogIn.UseVisualStyleBackColor = false;
            this.buttonLogIn.Click += new System.EventHandler(this.buttonLogIn_Click);
            // 
            // textInputAccount
            // 
            this.textInputAccount.Location = new System.Drawing.Point(328, 278);
            this.textInputAccount.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.textInputAccount.Name = "textInputAccount";
            this.textInputAccount.Size = new System.Drawing.Size(143, 25);
            this.textInputAccount.TabIndex = 1;
            this.textInputAccount.TextChanged += new System.EventHandler(this.textInputAccount_TextChanged);
            // 
            // textInputPassword
            // 
            this.textInputPassword.Location = new System.Drawing.Point(328, 342);
            this.textInputPassword.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.textInputPassword.Name = "textInputPassword";
            this.textInputPassword.Size = new System.Drawing.Size(143, 25);
            this.textInputPassword.TabIndex = 2;
            this.textInputPassword.TextChanged += new System.EventHandler(this.textInputPassword_TextChanged);
            // 
            // buttonRegister
            // 
            this.buttonRegister.BackColor = System.Drawing.Color.DarkSeaGreen;
            this.buttonRegister.Font = new System.Drawing.Font("黑体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.buttonRegister.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.buttonRegister.Location = new System.Drawing.Point(417, 412);
            this.buttonRegister.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonRegister.Name = "buttonRegister";
            this.buttonRegister.Size = new System.Drawing.Size(84, 29);
            this.buttonRegister.TabIndex = 5;
            this.buttonRegister.Text = "注册";
            this.buttonRegister.UseVisualStyleBackColor = false;
            this.buttonRegister.Click += new System.EventHandler(this.buttonRegister_Click);
            // 
            // LogIn
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::WeChattingClient.Properties.Resources.denglu;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(772, 469);
            this.Controls.Add(this.buttonRegister);
            this.Controls.Add(this.textInputPassword);
            this.Controls.Add(this.textInputAccount);
            this.Controls.Add(this.buttonLogIn);
            this.DoubleBuffered = true;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "LogIn";
            this.Text = "LogIn";
            this.Load += new System.EventHandler(this.LogIn_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonLogIn;
        private System.Windows.Forms.TextBox textInputAccount;
        private System.Windows.Forms.TextBox textInputPassword;
        private System.Windows.Forms.Button buttonRegister;
    }
}