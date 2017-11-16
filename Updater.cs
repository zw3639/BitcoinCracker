using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cracker
{
    public abstract class Updater
    {
        private string m_Url;

        public Updater(string url)
        {
            m_Url = url;
        }
        
        protected string ReadJson()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(m_Url);
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
