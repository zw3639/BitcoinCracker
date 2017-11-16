using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cracker
{
    public class Counter
    {
        private int m_Count;
        private DateTime m_Time;

        public Counter()
        {
            Reset();
        }

        public double Speed
        {
            get
            {
                int count = Interlocked.Exchange(ref m_Count, 0);
                DateTime lastTime = m_Time;
                m_Time = DateTime.Now;
                return count / (m_Time - lastTime).TotalSeconds;
            }
        }

        public void Reset()
        {
            m_Count = 0;
            m_Time = new DateTime();
        }

        public void Increment()
        {
            Interlocked.Increment(ref m_Count);
        }

        public void Decrement()
        {
            Interlocked.Decrement(ref m_Count);
        }
    }
}
