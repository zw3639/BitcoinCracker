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
    public class AddressUpdater : Updater
    {
        private DateTime m_Date;
        private int m_Count;
        private string m_Url;

        public AddressUpdater()
            : base("http://www.xiaonh.com/bitcoincracker/address.json")
        {
            m_Date = DateTime.MinValue;
            m_Count = 0;
            m_Url = null;
        }

        public DateTime Date {  get { return m_Date; } }
        public int Count { get { return m_Count; } }
        public string Url {  get { return m_Url; } }

        public bool Update()
        {
            try
            {
                string json = ReadJson();
                JObject obj = JsonConvert.DeserializeObject<JObject>(json);
                m_Date = (DateTime)obj["Date"];
                m_Count = (int)obj["Count"];
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
