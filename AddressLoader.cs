using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NBitcoin;

namespace Cracker
{
    public class AddressLoader
    {
        private static readonly string AddressPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Address.txt");
        private Action<DateTime, int> m_LoadAction;
        private Action m_LoadCompleteAction;
        private DateTime m_Date;
        private HashSet<KeyId> m_KeyIds;
        private volatile bool m_IsRunning;
        private Thread m_Thread;
        
        public AddressLoader(Action<DateTime, int> loadAction, Action loadCompleteAction)
        {
            m_LoadAction = loadAction;
            m_LoadCompleteAction = loadCompleteAction;
            m_Date = DateTime.MinValue;
            m_KeyIds = new HashSet<KeyId>();
            m_IsRunning = false;
            m_Thread = new Thread(new ThreadStart(Load));
        }
        
        public DateTime Date {  get { return m_Date; } }
        public HashSet<KeyId> KeyIds {  get { return m_KeyIds; } }

        public void Start()
        {
            m_IsRunning = true;
            m_Thread.Start();
        }

        public void Stop()
        {
            m_IsRunning = false;
            m_Thread.Join();
        }

        private void Load()
        {
            if (File.Exists(AddressPath))
            {
                try
                {
                    using (StreamReader reader = new StreamReader(AddressPath))
                    {
                        string line = reader.ReadLine();
                        m_Date = DateTime.Parse(line);

                        int count = 0;
                        while (m_IsRunning)
                        {
                            line = reader.ReadLine();
                            if (line == null)
                                break;

                            try
                            {
                                BitcoinPubKeyAddress address = new BitcoinPubKeyAddress(line);
                                m_KeyIds.Add(address.Hash);
                            }
                            catch
                            {
                            }

                            count++;
                            if (count >= 10000)
                            {
                                count = 0;
                                m_LoadAction?.Invoke(m_Date, m_KeyIds.Count);
                            }
                        }
                    }
                }
                catch
                {
                }
            }

            if (m_IsRunning)
            {
                m_LoadCompleteAction?.Invoke();
                m_IsRunning = false;
            }
        }
    }
}
