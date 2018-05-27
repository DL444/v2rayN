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

namespace v2rayNPF
{
    /// <summary>
    /// Interaction logic for EditCustomServerWindow.xaml
    /// </summary>
    public partial class EditCustomServerWindow : Window
    {
        public bool? ShowDialog(ref VmessItem server)
        {
            if(server == null) { return false; }
            AliasBox.Text = server.remarks;
            if(base.ShowDialog() == true)
            {
                server.remarks = AliasBox.Text;
                return true;
            }
            return false;
        }
        public EditCustomServerWindow()
        {
            InitializeComponent();
        }

        private void OkBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
