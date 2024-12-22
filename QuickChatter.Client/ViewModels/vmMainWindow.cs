using QuickChatter.Client.Helpers;
using QuickChatter.Client.Views.Controls;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace QuickChatter.Client.ViewModels
{
    public class vmMainWindow : vmBase
    {
        private TcpClient _client;
        private StreamWriter _writer;

        //Holds the current UserControl
        private object _currentControl;

        public object CurrentControl
        {
            get => _currentControl;
            set => SetProperty(ref _currentControl, value);
        }

        //Username
        private string _username;

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        //Holds the current UserControl
        private object _onlineUsers;

        public object OnlineUsers
        {
            get => _onlineUsers;
            set => SetProperty(ref _onlineUsers, value);
        }

        //Connect Click Event
        public ICommand ConnectCommand { get; }

        //Constructor
        public vmMainWindow()
        {
            ConnectCommand = new RelayCommand(ConnectToServer, CanButtonClick); ;
            CurrentControl = new ucConnect();

            Username = "eg. TheLegend27";
        }

        /// <summary>
        /// Method to connect to the server and listen to updates
        /// </summary>
        /// <param name="sender"></param>
        private async void ConnectToServer(object sender)
        {
            _client = new TcpClient("127.0.0.1", 5000);
            _writer = new StreamWriter(_client.GetStream(), Encoding.UTF8) { AutoFlush = true };

            //Connect with the server
            var isConnected = await ServerHelper.ConnectToServer(_client, _writer, Username);

            //If failed
            if (!isConnected)
            {
                //Show the client
                MessageBox.Show("Something went wrong while connecting to the server.");
            }
            else
            {
                //Listen for updates
                ServerHelper.ListenForUpdates(_client, this);

                //Navigate to the main screen
                CurrentControl = new ucMainScreen();
            }
        }

        private bool CanButtonClick(object parameter)
        {
            // Return true or false depending on whether the button should be enabled
            return true;
        }
    }
}
