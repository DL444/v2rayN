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
using Microsoft.Win32;
using System.ComponentModel;

namespace v2rayNPF
{
    /// <summary>
    /// Interaction logic for AddShadowsocksWindow.xaml
    /// </summary>
    public partial class AddShadowsocksWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public VmessItem TempItem
        {
            get => _tempItem;
            set
            {
                _tempItem = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TempItem"));
            }
        }
        VmessItem item = new VmessItem();
        private VmessItem _tempItem = new VmessItem();

        public bool? ShowDialog(ref VmessItem server)
        {
            if (server != null && !server.address.IsNullOrWhiteSpace())
            {
                TempItem = Utils.DeepCopy(server);
                item = server;
                SetComboBoxes();
            }
            else { server = new VmessItem(); }
            OkBtn.IsEnabled = ValidateValues();
            if (base.ShowDialog() == true)
            {
                server = item;
                return true;
            }
            else { return false; }
        }

        public AddShadowsocksWindow()
        {
            InitializeComponent();
        }

        private void OkBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateValues() == false) { MessageBox.Show("Config info invalid. ", "V2RayNPF", MessageBoxButton.OK); return; }
            item.configType = (int)EConfigType.Shadowsocks;
            item.configVersion = 2;
            DialogResult = true;
            item = TempItem;
            SetFromComboBoxes();
            this.Close();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }

        void SetComboBoxes()
        {
            for (int i = 0; i < EncryptionBox.Items.Count; i++)
            {
                if ((EncryptionBox.Items[i] as ComboBoxItem).Content as string == item.security)
                {
                    EncryptionBox.SelectedIndex = i;
                    return;
                }
            }
            EncryptionBox.SelectedIndex = 0;
        }
        void SetFromComboBoxes()
        {
            item.security = (EncryptionBox.SelectedItem as ComboBoxItem).Content as string;
        }
        bool ValidateValues()
        {
            if (AddressBox.Text.IsNullOrWhiteSpace()) { return false; }
            if (PortBox.Text.IsNullOrWhiteSpace() || !int.TryParse(PortBox.Text, out _)) { return false; }
            if (PasswordBox.Text.IsNullOrWhiteSpace()) { return false; }
            return true;
        }

        void Info_Changed(object sender, EventArgs e)
        {
            OkBtn.IsEnabled = ValidateValues();
        }
        void DefaultAll()
        {
            AddressBox.Text = "";
            PortBox.Text = "";
            PasswordBox.Text = "";
            EncryptionBox.SelectedIndex = 0;
            AliasBox.Text = "";
        }

        private void ImportClipboard()
        {
            string msg;
            VmessItem vmessItem = V2rayConfigHandler.ImportFromClipboardConfig(Utils.GetClipboardData(), out msg);
            if (vmessItem == null)
            {
                MessageBox.Show(msg, "V2RayNPF", MessageBoxButton.OK);
                return;
            }
            DefaultAll();
            TempItem = vmessItem;
        }

        private void ImportClipMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ImportClipboard();
        }
    }
}
