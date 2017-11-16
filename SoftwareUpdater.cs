using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cracker
{
    public class SoftwareUpdater : Updater
    {
        private Version m_Version;
        private string m_Url;

        public SoftwareUpdater()
            : base("http://www.xiaonh.com/bitcoincracker/software.json")
        {
            m_Version = new Version();
            m_Url = null;
        }

        public bool HasNew
        {
            get
            {
                AssemblyProductAttribute productAttr = (AssemblyProductAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyProductAttribute));
                string product = productAttr.Product;
                Version version = Assembly.GetExecutingAssembly().GetName().Version;
                return m_Version > version;
            }
        }
        public string Url {  get { return m_Url; } }

        public bool Update()
        {
            try
            {
                string json = ReadJson();
                JObject obj = JsonConvert.DeserializeObject<JObject>(json);
                m_Version = Version.Parse((string)obj["Version"]);
                m_Url = (string)obj["Url"];
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
