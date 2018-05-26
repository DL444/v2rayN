using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using v2rayNPF.Handler;
using v2rayNPF.Mode;
using System.ComponentModel;

namespace v2rayNPF
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public Config TempItem
        {
            get => _tempItem;
            set
            {
                _tempItem = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TempItem"));
            }
        }
        Config item = new Config();

        bool autoRun = Utils.IsAutoRun();
        private Config _tempItem = new Config();

        public bool ListenPort2Enabled { get; set; } = false;

        public string TempMTU { get; set; } = "";
        public string TempTTI { get; set; } = "";
        public string TempUpCap { get; set; } = "";
        public string TempDownCap { get; set; } = "";
        public string TempRdBufSize { get; set; } = "";
        public string TempWtBufSize { get; set; } = "";

        public bool? ShowDialog(ref Config config)
        {
            if (config != null)
            {
                TempItem = Utils.DeepCopy(config);
                item = config;
                SetControls();
            }
            else { config = new Config(); }
            //GetRoutingString();
            OkBtn.IsEnabled = ValidateValues();
            if (base.ShowDialog() == true)
            {
                config = item;
                return true;
            }
            else { return false; }
        }


        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }
        private void OkBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateValues() == false) { MessageBox.Show("Config info invalid. ", "V2RayNPF", MessageBoxButton.OK); return; }
            DialogResult = true;
            item = TempItem;
            SetFromControls();
            this.Close();
        }

        void SetControls()
        {
            for (int i = 0; i < LogLevelBox.Items.Count; i++)
            {
                if (item.loglevel == (LogLevelBox.Items[i] as ComboBoxItem).Content as string)
                {
                    LogLevelBox.SelectedIndex = i;
                }
            }
            SystemStartupToggle.IsChecked = autoRun;
            GetKcp();

            if (item.inbound == null || item.inbound.Count > 0)
            {
                item.inbound = new List<InItem>
                {
                    new InItem()
                };
            }
            ListenPort1Box.Text = item.inbound[0].localPort.ToString();
            ListenPort1ProtocolBox.SelectedIndex = (item.inbound[0].protocol == "socks") ? 0 : 1;
            ListenPort1UdpToggle.IsChecked = item.inbound[0].udpEnabled;
            ListenPort2Enabled = (item.inbound.Count > 1) ? true : false;
            if (ListenPort2Enabled)
            {
                ListenPort2Box.IsEnabled = true;
                ListenPort2ProtocolBox.IsEnabled = true;
                ListenPort2UdpToggle.IsEnabled = true;
                ListenPort2Box.Text = item.inbound[1].localPort.ToString();
                ListenPort2ProtocolBox.SelectedIndex = (item.inbound[1].protocol == "socks") ? 0 : 1;
                ListenPort2UdpToggle.IsChecked = item.inbound[1].udpEnabled;
            }

            GetRoutingString();
            //throw new NotImplementedException();
        }
        void SetFromControls()
        {
            item.loglevel = (LogLevelBox.SelectedItem as ComboBoxItem).Content as string;
            Utils.SetAutoRun(autoRun);
            SetKcp(true);

            int port1, port2;
            int.TryParse(ListenPort1Box.Text, out port1);
            item.inbound[0].localPort = port1;
            item.inbound[0].protocol = (ListenPort1ProtocolBox.SelectedItem as ComboBoxItem).Content as string;
            item.inbound[0].udpEnabled = ListenPort1UdpToggle.IsChecked.HasValue ? ListenPort1UdpToggle.IsChecked.Value : false;
            if (ListenPort2Enabled)
            {
                if (item.inbound.Count < 2) { item.inbound.Add(new InItem()); }
                int.TryParse(ListenPort2Box.Text, out port2);
                item.inbound[1].localPort = port2;
                item.inbound[1].protocol = (ListenPort2ProtocolBox.SelectedItem as ComboBoxItem).Content as string;
                item.inbound[1].udpEnabled = ListenPort2UdpToggle.IsChecked.HasValue ? ListenPort1UdpToggle.IsChecked.Value : false;
            }
            else
            {
                if (item.inbound.Count > 1)
                {
                    do
                    {
                        item.inbound.RemoveAt(1);
                    }
                    while (item.inbound.Count > 1);
                }
            }

            SetRoutingString();
            //throw new NotImplementedException();
        }
        bool ValidateValues()
        {
            if (SetKcp(false) == false) { return false; }
            if (ListenPort1Box.Text.IsNullOrWhiteSpace()) { return false; }
            if (int.TryParse(ListenPort1Box.Text, out _) == false) { return false; }
            if (ListenPort2Enabled)
            {
                if (ListenPort2Box.Text.IsNullOrWhiteSpace() || (int.TryParse(ListenPort1Box.Text, out _) == false))
                {
                    return false;
                }
            }
            return true;
            //throw new NotImplementedException();
        }


        public string ProxyRouteList { get; set; } = "";
        public string DirectRouteList { get; set; } = "";
        public string BlockedRouteList { get; set; } = "";
        void GetRoutingString()
        {
            ProxyRouteList = Utils.List2String(item.useragent);
            DirectRouteList = Utils.List2String(item.userdirect);
            BlockedRouteList = Utils.List2String(item.userblock);
        }
        void SetRoutingString()
        {
            item.useragent = Utils.String2List(ProxyRouteList.Trim());
            item.userdirect = Utils.String2List(DirectRouteList.Trim());
            item.userblock = Utils.String2List(BlockedRouteList.Trim());
            //throw new NotImplementedException();
        }

        private void SystemStartupToggle_Checked(object sender, RoutedEventArgs e)
        {
            autoRun = true;
        }

        private void SystemStartupToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            autoRun = false;
        }

        void GetKcp()
        {
            TempMTU = item.kcpItem.mtu.ToString();
            TempTTI = item.kcpItem.tti.ToString();
            TempUpCap = item.kcpItem.uplinkCapacity.ToString();
            TempDownCap = item.kcpItem.downlinkCapacity.ToString();
            TempRdBufSize = item.kcpItem.readBufferSize.ToString();
            TempWtBufSize = item.kcpItem.writeBufferSize.ToString();
        }
        bool SetKcp(bool set)
        {
            int mtu, tti, upCap, downCap, rdBuff, wtBuff;
            if (int.TryParse(TempMTU, out mtu) == false) { return false; }
            if (int.TryParse(TempTTI, out tti) == false) { return false; }
            if (int.TryParse(TempUpCap, out upCap) == false) { return false; }
            if (int.TryParse(TempDownCap, out downCap) == false) { return false; }
            if (int.TryParse(TempRdBufSize, out rdBuff) == false) { return false; }
            if (int.TryParse(TempWtBufSize, out wtBuff) == false) { return false; }
            if (set)
            {
                item.kcpItem.mtu = mtu;
                item.kcpItem.tti = tti;
                item.kcpItem.uplinkCapacity = upCap;
                item.kcpItem.downlinkCapacity = downCap;
                item.kcpItem.readBufferSize = rdBuff;
                item.kcpItem.writeBufferSize = wtBuff;
            }
            return true;
        }

        private void ListenPort2Toggle_Checked(object sender, RoutedEventArgs e)
        {
            ListenPort2Box.IsEnabled = true;
            ListenPort2ProtocolBox.IsEnabled = true;
            ListenPort2UdpToggle.IsEnabled = true;
        }

        private void ListenPort2Toggle_Unchecked(object sender, RoutedEventArgs e)
        {
            ListenPort2Box.IsEnabled = false;
            ListenPort2ProtocolBox.IsEnabled = false;
            ListenPort2UdpToggle.IsEnabled = false;
        }
    }
}
