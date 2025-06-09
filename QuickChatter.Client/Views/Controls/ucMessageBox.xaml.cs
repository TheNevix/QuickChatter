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

namespace QuickChatter.Client.Views.Controls
{
    /// <summary>
    /// Interaction logic for ucMessageBox.xaml
    /// </summary>
    public partial class ucMessageBox : UserControl
    {
        public event Action<bool> OnClose; // now returns a result

        public ucMessageBox(string title, string message)
        {
            InitializeComponent();
            MessageText.Text = message;
            TitleText.Text = title;
        }


        private void Ok_Click(object sender, RoutedEventArgs e) => OnClose?.Invoke(true);

        private void Close_Click(object sender, RoutedEventArgs e) => OnClose?.Invoke(false);
    }
}
