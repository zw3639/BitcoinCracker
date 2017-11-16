using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Cracker
{
    public class SimpleLog
    {
        private static readonly string LogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Cracker.Log");
        private static object LogLock = new object();
        private TextBox m_TextBox;

        public SimpleLog(TextBox textBox)
        {
            m_TextBox = textBox;
        }

        public void Log(string format, params object[] args)
        {
            lock (LogLock)
            {
                string s = string.Format("{0}: {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), string.Format(format, args));
                using (StreamWriter writer = new StreamWriter(LogPath, true))
                {
                    writer.WriteLine(s);
                }

                if (m_TextBox != null)
                    m_TextBox.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        m_TextBox.AppendText(s);
                        m_TextBox.AppendText("\n");
                    }));
            }
        }
    }
}
