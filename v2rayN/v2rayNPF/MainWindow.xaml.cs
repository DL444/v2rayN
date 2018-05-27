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
using System.Windows.Navigation;
using System.Windows.Shapes;
using v2rayNPF.Handler;
using v2rayNPF.HttpProxyHandler;
using v2rayNPF.Mode;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using System.ComponentModel;

namespace v2rayNPF.Forms
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window//, INotifyPropertyChanged
    {
        //public event PropertyChangedEventHandler PropertyChanged;
        protected Config config;// = new Config();

        public static ObservableCollection<VmessItem> vmessItems { get; private set; } = new ObservableCollection<VmessItem>();

        V2rayHandler v2RayHandler = new V2rayHandler();
        PACListHandle PACListHandle;
        V2rayUpdateHandle v2RayUpdateHandle;

        ContextMenu serverItemContextMenu;


        public MainWindow()
        {
            InitializeComponent();
            this.ShowInTaskbar = false;
            // TODO: Uncomment this.
            //this.WindowState = WindowState.Minimized;
            Application.Current.Exit += (sender, args) => { Utils.ClearTempPath(); };
            serverItemContextMenu = CreateServerItemContextMenu();
        }

        ContextMenu CreateServerItemContextMenu()
        {
            ContextMenu menu = new ContextMenu();
            MenuItem removeItem = new MenuItem() { Header = "Remove" };
            removeItem.Click += RemoveServerMenuItem_Click;
            MenuItem copyItem = new MenuItem() { Header = "Duplicate" };
            copyItem.Click += CopyServerMenuItem_Click;
            MenuItem activateItem = new MenuItem() { Header = "Activate" };
            activateItem.Click += ActivateServerMenuItem_Click;

            MenuItem moveTopItem = new MenuItem() { Header = "Move to Top" };
            moveTopItem.Click += MoveToTopMenuItem_Click;
            MenuItem moveUpItem = new MenuItem() { Header = "Move Up" };
            moveUpItem.Click += MoveUpMenuItem_Click;
            MenuItem moveDownItem = new MenuItem() { Header = "Move Down" };
            moveDownItem.Click += MoveDownMenuItem_Click;
            MenuItem moveBottomItem = new MenuItem() { Header = "Move to Bottom" };
            moveBottomItem.Click += MoveToBottomMenuItem_Click;

            MenuItem exportClientItem = new MenuItem() { Header = "Export as Client" };
            exportClientItem.Click += ExportAsClientMenuItem_Click;
            MenuItem exportServerItem = new MenuItem() { Header = "Export as Server" };
            exportServerItem.Click += ExportAsServerMenuItem_Click;

            menu.Items.Add(removeItem);
            menu.Items.Add(copyItem);
            menu.Items.Add(activateItem);
            menu.Items.Add(new Separator());
            menu.Items.Add(moveTopItem);
            menu.Items.Add(moveUpItem);
            menu.Items.Add(moveDownItem);
            menu.Items.Add(moveBottomItem);
            menu.Items.Add(new Separator());
            menu.Items.Add(exportClientItem);
            menu.Items.Add(exportServerItem);
            return menu;
        }

        private void MainWin_Loaded(object sender, RoutedEventArgs e)
        {
            ////ConfigHandler 
            ConfigHandler.LoadConfig(ref config);
            v2RayHandler.ProcessEvent += V2RayHandler_ProcessEvent;
            RefreshServers();
            LoadV2ray(false);
        }

        private void MainWin_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }
        private void MainWin_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                this.WindowState = WindowState.Normal;
                this.Hide();
            }
        }


        //void PopulateServerList()
        //{
        //    ServerBox.Items.Clear();

        //    for(int i = 0; i < config.vmess.Count; i++)
        //    {
        //        string selectMark = "";
        //        if(config.index.Equals(i))
        //        {
        //            selectMark = "√";
        //        }

        //        VmessItem item = config.vmess[i];

        //        //ServerBox.Items.Add(new List<string>() {
        //        //    selectMark,
        //        //    ((EConfigType)item.configType).ToString(),
        //        //    item.remarks,
        //        //    item.address,
        //        //    item.port.ToString(),
        //        //    item.security,
        //        //    item.network
        //        //});
        //    }
        //}

        int GetSelection()
        {
            try
            {
                if(ServerBox.SelectedIndex < 0)
                {
                    MessageBox.Show("No server selected.", "V2RayNPF", MessageBoxButton.OK);
                    return -1;
                }
                return ServerBox.SelectedIndex;
            }
            catch
            {
                return -1;
            }
        }
        void RefreshServers()
        {
            vmessItems = new ObservableCollection<VmessItem>(config.vmess);
            ServerBox.ItemsSource = vmessItems;
            ServerBox.UpdateLayout();
            for (int i = 0; i < ServerBox.Items.Count; i++)
            {
                ListViewItem serverItem = (ServerBox.ItemContainerGenerator.ContainerFromIndex(i) as ListViewItem);
                if (i == config.index)
                {
                    serverItem.FontWeight = FontWeights.Bold;
                }
                else
                {
                    serverItem.FontWeight = FontWeights.Regular;
                }
                serverItem.MouseDoubleClick += ServerItem_MouseDoubleClick;
                serverItem.KeyDown += ServerItem_KeyDown;
                serverItem.ContextMenu = serverItemContextMenu;
            }
        }

        private void ServerItem_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    ActivateServerMenuItem_Click(null, null);
                    break;
                case Key.Delete:
                    RemoveServerMenuItem_Click(null, null);
                    break;
                case Key.U:
                    MoveUpMenuItem_Click(null, null);
                    break;
                case Key.D:
                    MoveDownMenuItem_Click(null, null);
                    break;
            }
        }

        private void ServerItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int index = GetSelection();
            bool? confirmed = true;
            if (index < 0)
            {
                return;
            }
            VmessItem server = config.vmess[index];

            if (config.vmess[index].configType == (int)EConfigType.Vmess)
            {
                AddVMessWindow fm = new AddVMessWindow();
                confirmed = fm.ShowDialog(ref server);
            }
            else if (config.vmess[index].configType == (int)EConfigType.Shadowsocks)
            {
                AddShadowsocksWindow fm = new AddShadowsocksWindow();
                confirmed = fm.ShowDialog(ref server);
            }
            else
            {
                EditCustomServerWindow fm = new EditCustomServerWindow();
                confirmed = fm.ShowDialog(ref server);
            }
            if (confirmed == true)
            {
                config.vmess[index] = server;
                RefreshServers();
                LoadV2ray(true);
                ConfigHandler.ToJsonFile(config);
            }
        }

        void LoadV2ray(bool isReload)
        {
            if(isReload == true)
            {
                OutputBox.Clear();
            }
            v2RayHandler.LoadV2ray(config);
            ChangeSystemProxy(config.sysAgentEnabled);
        }
        void CloseV2Ray()
        {
            ConfigHandler.ToJsonFile(config);
            ChangeSystemProxy(false);
            v2RayHandler.V2rayStop();
        }

        void ChangeSystemProxy(bool enable)
        {
            if(enable)
            {
                if(HttpProxyHandle.RestartHttpAgent(config, true))
                {
                    ChangePACButtonStatus(config.listenerType);
                }
            }
            else
            {
                HttpProxyHandle.Update(config, true);
                HttpProxyHandle.CloseHttpAgent(config);
            }

            SystemProxyMenuToggle.IsChecked = SystemProxyModeMenuItem.IsEnabled = enable;
        }
        void ChangePACButtonStatus(int type)
        {
            if(HttpProxyHandle.Update(config, false))
            {
                switch(type)
                {
                    case 0:
                        UnchangeProxyMenuItem.IsChecked = true;
                        GlobalProxyMenuItem.IsChecked = false;
                        PACProxyMenuItem.IsChecked = false;
                        break;
                    case 1:
                        UnchangeProxyMenuItem.IsChecked = false;
                        GlobalProxyMenuItem.IsChecked = true;
                        PACProxyMenuItem.IsChecked = false;
                        break;
                    case 2:
                        UnchangeProxyMenuItem.IsChecked = false;
                        GlobalProxyMenuItem.IsChecked = false;
                        PACProxyMenuItem.IsChecked = true;
                        break;
                }
            }
        }

        int SetDefaultServer(int index)
        {
            if (index < 0)
            {
                MessageBox.Show("No server selected.");
                return -1;
            }
            if (ConfigHandler.SetDefaultServer(ref config, index) == 0)
            {
                RefreshServers();
                LoadV2ray(true);
            }
            return 0;
        }


        private void GlobalProxyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            config.listenerType = 1;
            ChangePACButtonStatus(1);
        }

        private void PACProxyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            config.listenerType = 2;
            ChangePACButtonStatus(2);
        }

        private void UnchangeProxyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            config.listenerType = 0;
            ChangePACButtonStatus(0);
        }

        private void SystemProxyMenuToggle_Click(object sender, RoutedEventArgs e)
        {
            config.sysAgentEnabled = !config.sysAgentEnabled;
            ChangeSystemProxy(config.sysAgentEnabled);
        }

        private void CopyPACMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Utils.SetClipboardData(HttpProxyHandle.GetPacUrl());
        }


        void MoveServer(int index, EMove direction)
        {
            if(ConfigHandler.MoveServer(ref config, index, direction) == 0)
            {
                RefreshServers();
                LoadV2ray(true);
            }
        }
        private void MoveToTopMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MoveServer(GetSelection(), EMove.Top);
        }

        private void MoveUpMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MoveServer(GetSelection(), EMove.Up);
        }

        private void MoveDownMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MoveServer(GetSelection(), EMove.Down);
        }

        private void MoveToBottomMenuItem_Click(object sender, RoutedEventArgs e)
        {
           MoveServer(GetSelection(), EMove.Bottom);
        }


        private void V2RayHandler_ProcessEvent(bool notify, string msg)
        {
            AppendConsole(msg);
        }
        void AppendConsole(string text)
        {
            this.Dispatcher.Invoke(() =>
            {
                OutputBox.AppendText(text);
                if (!text.EndsWith("\r\n")) { OutputBox.AppendText("\r\n"); }
            });
        }
        void ClearConsole()
        {
            OutputBox.Clear();
        }


        DrawingImage GetQrCode(ref string url, int index, Config config)
        {
            if(index >= 0)
            {
                url = ConfigHandler.GetVmessQRCode(config, index);
                if(url.IsNullOrEmpty())
                {
                    url = "";
                    return null;
                }
                return QRCodeHelper.GetQRCode(url);
            }
            url = "";
            return null;
        }
        private void ServerBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ServerBox.SelectedIndex >= 0)
            {
                string url = "";
                QrImage.Source = GetQrCode(ref url, ServerBox.SelectedIndex, config);
                AddressBox.Text = url;
            }
        }


        private void ServerSubmenuItem_Click(object sender, RoutedEventArgs e)
        {
            SetDefaultServer((int)((sender as MenuItem).Tag));
        }
        private void NotifyIcon_TrayContextMenuOpen(object sender, RoutedEventArgs e)
        {
            List<MenuItem> serverMenu = new List<MenuItem>();
            ServersMenuItem.Items.Clear();
            if (config.vmess == null || config.vmess.Count == 0)
            {
                MenuItem placeholder = new MenuItem();
                placeholder.Header = "No server";
                placeholder.IsEnabled = false;
                ServersMenuItem.Items.Add(placeholder);
                return;
            }
            for (int i = 0; i < config.vmess.Count; i++)
            {
                MenuItem item = new MenuItem();
                item.Tag = i;
                item.Header = config.vmess[i].getSummary();
                item.IsCheckable = true;
                if (i == config.index) { item.IsChecked = true; }
                item.Click += ServerSubmenuItem_Click;
                ServersMenuItem.Items.Add(item);
            }
        }

        private void RemoveServerMenuItem_Click(object sender, RoutedEventArgs e)
        {
            int index = -1;
            if((index = ServerBox.SelectedIndex) < 0) { return; }
            if(MessageBox.Show("Remove selected server?", "V2RayNPF", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                if(ConfigHandler.RemoveServer(ref config, ServerBox.SelectedIndex) == 0)
                {
                    RefreshServers();
                    LoadV2ray(true);
                }
            }
        }

        private void CopyServerMenuItem_Click(object sender, RoutedEventArgs e)
        {
            int index = -1;
            if ((index = ServerBox.SelectedIndex) < 0) { return; }
            ConfigHandler.CopyServer(ref config, ServerBox.SelectedIndex);
            RefreshServers();
        }

        private void ActivateServerMenuItem_Click(object sender, RoutedEventArgs e)
        {
            int index = -1;
            if ((index = ServerBox.SelectedIndex) < 0) { return; }
            ConfigHandler.SetDefaultServer(ref config, ServerBox.SelectedIndex);
            RefreshServers();
            LoadV2ray(true);
        }

        private void ExportAsClientMenuItem_Click(object sender, RoutedEventArgs e)
        {
            int index = ServerBox.SelectedIndex;
            if (index < 0)
            {
                return;
            }
            if (config.vmess[index].configType != (int)EConfigType.Vmess)
            {
                MessageBox.Show("This feature is only valid for VMess servers.");
                return;
            }

            SaveFileDialog fileDialog = new SaveFileDialog();
            fileDialog.Filter = "Config|*.json";
            fileDialog.FilterIndex = 2;
            fileDialog.RestoreDirectory = true;
            if (fileDialog.ShowDialog() != true)
            {
                return;
            }
            string fileName = fileDialog.FileName;
            if (Utils.IsNullOrEmpty(fileName))
            {
                return;
            }
            Config configCopy = Utils.DeepCopy<Config>(config);
            configCopy.index = index;
            if (V2rayConfigHandler.Export2ClientConfig(configCopy, fileName, out string msg) != 0)
            {
                // TODO: Check translation
                MessageBox.Show(msg, "V2RayNPF", MessageBoxButton.OK);
            }
            else
            {
                MessageBox.Show($"Client config saved as {fileName}");
            }
        }

        private void ExportAsServerMenuItem_Click(object sender, RoutedEventArgs e)
        {
            int index = ServerBox.SelectedIndex;
            if (index < 0)
            {
                return;
            }
            if (config.vmess[index].configType != (int)EConfigType.Vmess)
            {
                MessageBox.Show("This feature is only valid for VMess servers.");
                return;
            }

            SaveFileDialog fileDialog = new SaveFileDialog();
            fileDialog.Filter = "Config|*.json";
            fileDialog.FilterIndex = 2;
            fileDialog.RestoreDirectory = true;
            if (fileDialog.ShowDialog() != true)
            {
                return;
            }
            string fileName = fileDialog.FileName;
            if (Utils.IsNullOrEmpty(fileName))
            {
                return;
            }
            Config configCopy = Utils.DeepCopy<Config>(config);
            configCopy.index = index;
            string msg;
            if (V2rayConfigHandler.Export2ServerConfig(configCopy, fileName, out msg) != 0)
            {
                MessageBox.Show(msg, "V2RayNPF", MessageBoxButton.OK);
            }
            else
            {
                MessageBox.Show($"Client config saved as { fileName}");
            }
        }

        private void ExportToClipMenuItem_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            for (int k = 0; k < config.vmess.Count; k++)
            {
                string url = ConfigHandler.GetVmessQRCode(config, k);
                if (Utils.IsNullOrEmpty(url))
                {
                    continue;
                }
                sb.Append(url);
                sb.AppendLine();
            }
            if (sb.Length > 0)
            {
                Utils.SetClipboardData(sb.ToString());
                MessageBox.Show("Exported to clipboard.");
            }
        }

        private void RestartMenu_Click(object sender, RoutedEventArgs e)
        {
            LoadV2ray(true);
        }

        private void MinimizeMenu_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        private void AddVmessMenuItem_Click(object sender, RoutedEventArgs e)
        {
            VmessItem item = null;
            AddVMessWindow addServerWindow = new AddVMessWindow();
            if(addServerWindow.ShowDialog(ref item) == true)
            {
                if(ConfigHandler.AddServer(ref config, item, -1) == 0)
                {
                    RefreshServers();
                    LoadV2ray(true);
                }
                else
                {
                    MessageBox.Show("Operation Failed. Please Check Your Configuration.", "V2RayNPF", MessageBoxButton.OK);
                }
            }
        }

        private void AddSSMenuItem_Click(object sender, RoutedEventArgs e)
        {
            VmessItem item = null;
            AddShadowsocksWindow addServerWindow = new AddShadowsocksWindow();
            if (addServerWindow.ShowDialog(ref item) == true)
            {
                if (ConfigHandler.AddServer(ref config, item, -1) == 0)
                {
                    RefreshServers();
                    LoadV2ray(true);
                }
                else
                {
                    MessageBox.Show("Operation Failed. Please Check Your Configuration.", "V2RayNPF", MessageBoxButton.OK);
                }
            }
        }

        private void ShareToggle_Checked(object sender, RoutedEventArgs e)
        {
            QrImage.Visibility = Visibility.Visible;
            AddressBox.Visibility = Visibility.Visible;
            CopyAddressBtn.Visibility = Visibility.Visible;
        }

        private void ShareToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            QrImage.Visibility = Visibility.Collapsed;
            AddressBox.Visibility = Visibility.Collapsed;
            CopyAddressBtn.Visibility = Visibility.Collapsed;
        }

        private void SettingsMenu_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow();
            if(settingsWindow.ShowDialog(ref config) == true)
            {
                ConfigHandler.SaveConfig(ref config);
                RefreshServers();
                LoadV2ray(true);
            }
        }

        private void CopyAddressBtn_Click(object sender, RoutedEventArgs e)
        {
            Utils.SetClipboardData(AddressBox.Text);
        }

        private void NotifyIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Normal;
            this.Show();
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            CloseV2Ray();
            Application.Current.Shutdown();
        }

        private void ImportFromClipMenuItem_Click(object sender, RoutedEventArgs e)
        {
            string clipboardData = Utils.GetClipboardData();
            if (Utils.IsNullOrEmpty(clipboardData))
            {
                return;
            }
            int countServers = 0;
            string[] arrData = clipboardData.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            foreach (string str in arrData)
            {
                string msg;
                VmessItem vmessItem = V2rayConfigHandler.ImportFromClipboardConfig(str, out msg);
                if (vmessItem == null)
                {
                    continue;
                }
                if (vmessItem.configType == (int)EConfigType.Vmess)
                {
                    if (ConfigHandler.AddServer(ref config, vmessItem, -1) == 0)
                    {
                        countServers++;
                    }
                }
                else if (vmessItem.configType == (int)EConfigType.Shadowsocks)
                {
                    if (ConfigHandler.AddShadowsocksServer(ref config, vmessItem, -1) == 0)
                    {
                        countServers++;
                    }
                }
            }
            if (countServers > 0)
            {
                RefreshServers();
                //MessageBox.Show("Import Success.", "V2RayNPF", MessageBoxButton.OK);
            }
        }

        private void AddCustomServerMenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = false;
            fileDialog.Filter = "Config|*.json";
            if (fileDialog.ShowDialog() != true)
            {
                return;
            }
            string fileName = fileDialog.FileName;
            if (Utils.IsNullOrEmpty(fileName))
            {
                return;
            }

            if (ConfigHandler.AddCustomServer(ref config, fileName) == 0)
            {
                RefreshServers();
                LoadV2ray(true);
            }
            else
            {
                MessageBox.Show(string.Format("Failed to import custom configuraion."));
            }
        }
    }

    [ValueConversion(typeof(int), typeof(string))]
    public class ServerTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch ((int)value)
            {
                case 1:
                    return "VMess";
                case 2:
                    return "Custom";
                case 3:
                    return "Shadowsocks";
                default:
                    return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

}
