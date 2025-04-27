using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WeChattingClient
{
    static class Program
    {
        
        [STAThread]//单线程单元模型 使用 Windows 窗体和其他 COM 组件（如剪贴板、拖放等）所必需的
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            LogIn login = new LogIn();

            Application.Run(login) ;
        }
    }
}
