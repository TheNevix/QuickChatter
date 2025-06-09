using QuickChatter.Client.ViewModels;
using QuickChatter.Client.Views.Controls;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;

namespace QuickChatter.Client.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var chrome = new WindowChrome
            {
                CaptionHeight = 0,
                CornerRadius = new CornerRadius(0),
                GlassFrameThickness = new Thickness(0),
                ResizeBorderThickness = new Thickness(6),
                UseAeroCaptionButtons = false
            };

            WindowChrome.SetWindowChrome(this, chrome);

            DataContext = new vmMainWindow(this);
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        public Task<bool> ShowMessageBox(string title, string message)
        {
            var tcs = new TaskCompletionSource<bool>();

            var msgBox = new ucMessageBox(title, message);
            msgBox.OnClose += result =>
            {
                DialogHost.Visibility = Visibility.Collapsed;
                DialogHost.Content = null;
                tcs.SetResult(result);
            };

            DialogHost.Content = msgBox;
            DialogHost.Visibility = Visibility.Visible;

            return tcs.Task;
        }

    }
}