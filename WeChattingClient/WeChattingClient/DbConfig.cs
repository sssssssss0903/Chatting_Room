using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace WeChattingClient
{
    public static class DbConfig
    {
        public static string UID { get; set; } = "";
        public static string Password { get; set; } = "";

        public static string Host { get; set; } = "localhost";
        public static string Port { get; set; } = "3306";
        public static string Database { get; set; } = "wechatting";
        public static string User { get; set; } = "root";
        public static string DbPassword { get; set; } = "123456";

        public static string GetConnectionString()
        {
            return $"server={Host};port={Port};database={Database};user id={User};password={DbPassword};pooling=true;charset=utf8;";
        }

        public static void LoadFromXml(string xmlPath = "client_config.xml")
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(xmlPath);

                XmlNode root = doc.SelectSingleNode("ClientConfig");
                UID = root.SelectSingleNode("UID")?.InnerText ?? "";
                Password = root.SelectSingleNode("Password")?.InnerText ?? "";

                Host = root.SelectSingleNode("Database/Host")?.InnerText ?? "localhost";
                Port = root.SelectSingleNode("Database/Port")?.InnerText ?? "3306";
                Database = root.SelectSingleNode("Database/Name")?.InnerText ?? "wechatting";
                User = root.SelectSingleNode("Database/User")?.InnerText ?? "root";
                DbPassword = root.SelectSingleNode("Database/DbPassword")?.InnerText ?? "123456";
            }
            catch (Exception ex)
            {
                MessageBox.Show("读取数据库配置失败：" + ex.Message);
            }
        }
    }

}
