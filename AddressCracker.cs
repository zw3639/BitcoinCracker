using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using NBitcoin;

namespace Cracker
{
    public class AddressCracker
    {
        private AddressLoader m_Loader;
        private SimpleLog m_Log;
        private Counter m_Counter;
        private volatile bool m_IsRunning;
        private Thread m_Thread;

        public AddressCracker(AddressLoader loader, SimpleLog log, Counter counter)
        {
            m_Loader = loader;
            m_Log = log;
            m_Counter = counter;
            m_IsRunning = false;
            m_Thread = new Thread(new ThreadStart(Crack));
        }

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

        public void Crack()
        {
            byte[] data = new byte[KEY_SIZE];
            CreateData(data);

            while (m_IsRunning)
            {
                Key key = new Key(data);
                BitcoinPubKeyAddress address = key.PubKey.GetAddress(Network.Main);
                if (m_Loader.KeyIds.Contains(address.Hash))
                    m_Log.Log(Cracker.Properties.Resources.CrackSuccessfulFormat, address, key.ToString(Network.Main));

                IncreaseData(data);
                m_Counter.Increment();
            }
        }

        #region Key Data Generator
        private const int KEY_SIZE = 32;
        private readonly static uint256 N = uint256.Parse("fffffffffffffffffffffffffffffffebaaedce6af48a03bbfd25e8cd0364141");

        private static bool Check(byte[] data)
        {
            uint256 n = new uint256(data);
            return n > 0 && n < N;
        }

        private static void CreateData(byte[] data)
        {
            do
            {
                RandomUtils.GetBytes(data);
            } while (!Check(data));
        }

        private static void IncreaseData(byte[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] == 0xFF)
                    data[i] = 0;
                else
                {
                    data[i]++;
                    break;
                }
            }

            if (!Check(data))
                CreateData(data);
        }
        #endregion
    }
}
