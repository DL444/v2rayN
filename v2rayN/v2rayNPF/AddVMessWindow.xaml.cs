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
    /// Interaction logic for AddVMessWindow.xaml
    /// </summary>
    public partial class AddVMessWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public bool? ShowDialog(ref VmessItem server)
        {
            // TODO: Distinguish Add/Edit on caller side. Also, handle cancel on caller side.
            if (server != null && !server.address.IsNullOrWhiteSpace())
            {
                TempItem = Utils.DeepCopy(server);
                item = server;
                SetControls();
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
        public VmessItem TempItem
        {
            get => _tempItem;
            set
            {
                _tempItem = value;
                OnPropertyChanged("TempItem");
            }
        }
        VmessItem item = new VmessItem();
        private VmessItem _tempItem = new VmessItem();

        public AddVMessWindow()
        {
            InitializeComponent();
        }

        private void GenerateGuidBtn_Click(object sender, RoutedEventArgs e)
        {
            UserIdBox.Text = Utils.GetGUID();
            OkBtn.IsEnabled = ValidateValues();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }

        private void OkBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateValues() == false) { MessageBox.Show("Config info invalid. ", "V2RayNPF", MessageBoxButton.OK); return; }
            item.configType = (int)EConfigType.Vmess;
            item.configVersion = 2;
            DialogResult = true;
            item = TempItem;
            SetFromControls();
            this.Close();
        }

        void SetControls()
        {
            switch (item.security)
            {
                case "aes-128-cfb":
                    EncryptionBox.SelectedIndex = 0;
                    break;
                case "aes-128-gcm":
                    EncryptionBox.SelectedIndex = 1;
                    break;
                case "chacha20-poly1305":
                    EncryptionBox.SelectedIndex = 2;
                    break;
                case "none":
                    EncryptionBox.SelectedIndex = 3;
                    break;
                default:
                    EncryptionBox.SelectedIndex = 1;
                    break;
            }
            switch (item.network)
            {
                case "tcp":
                    EncryptionBox.SelectedIndex = 0;
                    break;
                case "kcp":
                    EncryptionBox.SelectedIndex = 1;
                    break;
                case "ws":
                    EncryptionBox.SelectedIndex = 2;
                    break;
                case "h2":
                    EncryptionBox.SelectedIndex = 3;
                    break;
                default:
                    EncryptionBox.SelectedIndex = 0;
                    break;
            }
        }
        void SetFromControls()
        {
            item.security = (EncryptionBox.SelectedItem as ComboBoxItem).Content as String;
            item.network = (NetworkBox.SelectedItem as ComboBoxItem).Content as String;
        }

        bool ValidateValues()
        {
            if (AddressBox.Text.IsNullOrWhiteSpace()) { return false; }
            if (PortBox.Text.IsNullOrWhiteSpace() || !int.TryParse(PortBox.Text, out _)) { return false; }
            if (UserIdBox.Text.IsNullOrWhiteSpace() || !Guid.TryParse(UserIdBox.Text, out _)) { return false; }
            if (AlterIdBox.Text.IsNullOrWhiteSpace() || !int.TryParse(AlterIdBox.Text, out _)) { return false; }
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
            UserIdBox.Text = "";
            AlterIdBox.Text = "";
            EncryptionBox.SelectedIndex = 1;
            NetworkBox.SelectedIndex = 0;
            AliasBox.Text = "";
        }

        private void ImportConfig(int type)
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
            string msg;
            VmessItem item = null;
            // TODO: Deal with this msg thing.
            if (type.Equals(1))
            {
                item = V2rayConfigHandler.ImportFromClientConfig(fileName, out msg);
            }
            else
            {
                item = V2rayConfigHandler.ImportFromServerConfig(fileName, out msg);
            }
            if (item == null)
            {
                MessageBox.Show(msg, "V2RayNPF", MessageBoxButton.OK);
                return;
            }
            DefaultAll();
            this.item = item;
            SetControls();
            TempItem = item;
            OkBtn.IsEnabled = ValidateValues();
        }

        private void ImportClientMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ImportConfig(1);
        }

        private void ImportServerMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ImportConfig(2);
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
