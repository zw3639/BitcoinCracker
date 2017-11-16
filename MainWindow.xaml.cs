using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Cracker
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private System.Windows.Forms.NotifyIcon m_NotifyIcon;
        private System.Windows.Forms.ContextMenuStrip m_ContextMenuStrip;
        private bool m_CancelClose;
        private volatile bool m_IsLoaded;
        private volatile bool m_IsStarted;
        private SimpleLog m_Log;
        private AddressLoader m_Loader;
        private List<AddressCracker> m_Crackers;
        private Counter m_Counter;
        private DispatcherTimer m_CrackTimer;
        private DispatcherTimer m_UpdateTimer;

        public MainWindow()
        {
            InitializeComponent();

            m_NotifyIcon = new System.Windows.Forms.NotifyIcon();
            m_NotifyIcon.Text = Cracker.Properties.Resources.Title;
            m_NotifyIcon.Icon = Cracker.Properties.Resources.xiaonh;
            m_NotifyIcon.MouseClick += M_NotifyIcon_MouseClick;
            m_NotifyIcon.Visible = true;
            m_ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
            m_ContextMenuStrip.Items.Add(new System.Windows.Forms.ToolStripMenuItem(Cracker.Properties.Resources.ShowWindow, null, new EventHandler(miShow_Click)));
            m_ContextMenuStrip.Items.Add(new System.Windows.Forms.ToolStripMenuItem(Cracker.Properties.Resources.HideWindow, null, new EventHandler(miHide_Click)));
            m_ContextMenuStrip.Items.Add(new System.Windows.Forms.ToolStripSeparator());
            m_ContextMenuStrip.Items.Add(new System.Windows.Forms.ToolStripMenuItem(Cracker.Properties.Resources.Exit, null, new EventHandler(miExit_Click)));
            m_NotifyIcon.ContextMenuStrip = m_ContextMenuStrip;
            m_CancelClose = true;
            
            m_IsLoaded = false;
            m_IsStarted = false;
            m_Log = new SimpleLog(tbOutput);
            m_Loader = new AddressLoader(LoadCallback, LoadCompleteCallback);
            m_Crackers = new List<AddressCracker>();
            m_Counter = new Counter();
            m_CrackTimer = new DispatcherTimer();
            m_CrackTimer.Tick += m_CrackTimer_Tick;
            m_CrackTimer.Interval = TimeSpan.FromSeconds(1);
            m_UpdateTimer = new DispatcherTimer();
            m_UpdateTimer.Tick += M_UpdateTimer_Tick;
            m_UpdateTimer.Interval = TimeSpan.FromMinutes(1);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            int defaultCount = (Environment.ProcessorCount + 1) / 2;
            for (int i = 1; i <= Environment.ProcessorCount; i++)
            {
                ComboBoxItem item = new ComboBoxItem();
                item.Content = i.ToString();
                item.Tag = i;
                item.IsSelected = (i == defaultCount);
                cbThreadCount.Items.Add(item);
            }

            m_Log.Log(Cracker.Properties.Resources.StartToLoadAddress);
            m_Loader.Start();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = m_CancelClose;
            if (m_CancelClose)
            {
                this.Visibility = Visibility.Collapsed;
                this.ShowInTaskbar = false;

                m_NotifyIcon.BalloonTipText = Cracker.Properties.Resources.SystemTrayTip;
                m_NotifyIcon.ShowBalloonTip(6000);
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Stop();
            m_UpdateTimer.Stop();
            m_NotifyIcon.Dispose();
            m_Loader.Stop();
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (cbThreadCount.SelectedIndex == -1)
            {
                MessageBox.Show(this, Cracker.Properties.Resources.ThreadCountTip, Cracker.Properties.Resources.Tip);
                return;
            }

            m_IsStarted = true;
            Start();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            Stop();
        }

        private void M_NotifyIcon_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if ((e.Button & System.Windows.Forms.MouseButtons.Left) != 0)
            {
                this.Visibility = Visibility.Visible;
                this.ShowInTaskbar = true;
                this.Activate();
            }
        }

        private void m_CrackTimer_Tick(object sender, EventArgs e)
        {
            double speed = m_Counter.Speed;
            tbSpeed.Text = string.Format("{0}{1}", Math.Round(speed), Cracker.Properties.Resources.CrackSpeedUnit);
            double probability = m_Loader.KeyIds.Count * speed * 3600 * 24 / Math.Pow(2, 160);
            tbProbability.Text = string.Format("{0:G3}", probability);
        }

        private void M_UpdateTimer_Tick(object sender, EventArgs e)
        {
            CheckAddressUpdate();
            CheckSoftwareUpdate();
        }

        #region Menu Event
        private void miAddress_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mi = (MenuItem)sender;
            string url = (string)mi.Tag;
            try
            {
                Process.Start(url);
            }
            catch
            {

            }
        }

        private void miSoftware_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mi = (MenuItem)sender;
            string url = (string)mi.Tag;
            try
            {
                Process.Start(url);
            }
            catch
            {

            }
        }

        private void miHelp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("http://www.xiaonh.com/index.php/2017/10/30/bitcoincracker/");
            }
            catch
            {

            }
        }

        private void miAbout_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow w = new AboutWindow();
            w.Owner = this;
            w.ShowDialog();
        }
        #endregion

        #region System Tray Event
        private void miShow_Click(object sender, EventArgs e)
        {
            this.Visibility = Visibility.Visible;
            this.ShowInTaskbar = true;
            this.Activate();
        }

        private void miHide_Click(object sender, EventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            this.ShowInTaskbar = false;
        }

        private void miExit_Click(object sender, EventArgs e)
        {
            Stop();
            m_CancelClose = false;
            this.Close();
        }
        #endregion

        private void Start()
        {
            if (m_IsLoaded && m_Loader.KeyIds.Count == 0)
            {
                m_Log.Log(Cracker.Properties.Resources.NoAddressTip);

                cbThreadCount.IsEnabled = false;
                btnStart.IsEnabled = false;
                btnStop.IsEnabled = false;
                return;
            }

            if (m_IsStarted)
            {
                if (m_IsLoaded)
                {
                    m_Log.Log(Cracker.Properties.Resources.StartCrack);
                    m_IsStarted = false;
                    m_CrackTimer.Start();
                    int threadCount = (int)((ComboBoxItem)cbThreadCount.SelectedItem).Tag;
                    for (int i = 0; i < threadCount; i++)
                    {
                        AddressCracker cracker = new AddressCracker(m_Loader, m_Log, m_Counter);
                        cracker.Start();
                        m_Crackers.Add(cracker);
                    }

                    cbThreadCount.IsEnabled = false;
                    btnStart.IsEnabled = false;
                    btnStop.IsEnabled = true;
                }
                else
                {
                    m_Log.Log(Cracker.Properties.Resources.CrackAfterAddressLoaded);

                    cbThreadCount.IsEnabled = false;
                    btnStart.IsEnabled = false;
                    btnStop.IsEnabled = true;
                }
            }
        }

        private void Stop()
        {
            m_IsStarted = false;
            m_CrackTimer.Stop();
            m_Counter.Reset();
            foreach (AddressCracker cracker in m_Crackers)
            {
                cracker.Stop();
            }
            m_Crackers.Clear();
            m_Log.Log(Cracker.Properties.Resources.StopCrack);

            cbThreadCount.IsEnabled = true;
            tbSpeed.Text = "--";
            tbProbability.Text = "--";
            btnStart.IsEnabled = true;
            btnStop.IsEnabled = false;
        }

        private void CheckAddressUpdate()
        {
            AddressUpdater updater = new AddressUpdater();
            if (!updater.Update() || (updater.Date < m_Loader.Date) || (updater.Date == m_Loader.Date && updater.Count <= m_Loader.KeyIds.Count))
            {
                miAddress.Visibility = Visibility.Collapsed;
                miAddress.Tag = null;
                return;
            }
            miAddress.Visibility = Visibility.Visible;
            miAddress.Tag = updater.Url;
        }

        private void CheckSoftwareUpdate()
        {
            SoftwareUpdater updater = new SoftwareUpdater();
            if (!updater.Update() || !updater.HasNew)
            {
                miSoftware.Visibility = Visibility.Collapsed;
                miSoftware.Tag = null;
                return;
            }
            miSoftware.Visibility = Visibility.Visible;
            miSoftware.Tag = updater.Url;
        }

        private void LoadCallback(DateTime date, int count)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (count > 0)
                    tbAddress.Text = string.Format(Cracker.Properties.Resources.AddressLoadingFormat, date, count);
            }));
        }
        
        private void LoadCompleteCallback()
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                m_Log.Log(Cracker.Properties.Resources.FinishLoadAddress);
                if (m_Loader.KeyIds.Count == 0)
                    tbAddress.Text = Cracker.Properties.Resources.NoAddressTip;
                else
                    tbAddress.Text = string.Format(Cracker.Properties.Resources.AddressLoadedFormat, m_Loader.Date, m_Loader.KeyIds.Count);

                M_UpdateTimer_Tick(null, null);
                m_UpdateTimer.Start();
                m_IsLoaded = true;
                Start();
            }));
        }
    }
}
