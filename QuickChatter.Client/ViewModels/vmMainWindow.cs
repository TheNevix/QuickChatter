using QuickChatter.Client.Helpers;
using QuickChatter.Client.Views;
using QuickChatter.Client.Views.Controls;
using QuickChatter.Models;
using QuickChatter.Models.Settings;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace QuickChatter.Client.ViewModels
{
    public class vmMainWindow : vmBase
    {
        public ICommand MinimizeCommand { get; }
        public ICommand MaximizeRestoreCommand { get; }
        public ICommand CloseCommand { get; }

        private TcpClient _client;
        private StreamWriter _writer;

        //Indicates if the user is connected or not
        private bool IsConnected => _client?.Connected == true && _writer != null;

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

        //Password
        private string _password;

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        //RegisterUsername
        private string _registerUsername;

        public string RegisterUsername
        {
            get => _registerUsername;
            set => SetProperty(ref _registerUsername, value);
        }

        //RegisterPassword
        private string _registerPassword;

        public string RegisterPassword
        {
            get => _registerPassword;
            set => SetProperty(ref _registerPassword, value);
        }

        //Entered Message
        private string _message;

        public string ConvoMessage
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        //Holds the current UserControl
        private ObservableCollection<User> _onlineUsers;

        public ObservableCollection<User> OnlineUsers
        {
            get => _onlineUsers;
            set => SetProperty(ref _onlineUsers, value);
        }

        //Holds the current UserControl
        private ObservableCollection<ConversationMessage> _conversationMessages;

        public ObservableCollection<ConversationMessage> ConversationMessages
        {
            get => _conversationMessages;
            set => SetProperty(ref _conversationMessages, value);
        }

        public Conversation _conversation;
        public Conversation Conversation
        {
            get => _conversation;
            set => SetProperty(ref _conversation, value);
        }

        //Selected user from the list of online users
        private User _selectedUser;

        public User SelectedUser
        {
            get { return _selectedUser; }
            set
            {
                if (_selectedUser != value)
                {
                    _selectedUser = value;
                    OnPropertyChanged(nameof(SelectedUser)); // Notify that the property changed
                }
            }
        }

        public string ConversationId;
        public string UserId;


        //Connect Click Event
        public ICommand ConnectCommand { get; }

        //Register Click Event
        public ICommand RegisterCommand { get; }

        //Invite user for conversation Event
        public ICommand InviteForConversationCommand { get; }

        //Send conversation message
        public ICommand SendConversationMessageCommand { get; }
        //Back to menu/End conversation
        public ICommand BackToMenuCommand { get; }

        public ICommand KeyDownCommand { get; }

        private readonly Window _window;

        //Constructor
        public vmMainWindow(Window window)
        {
            ConnectCommand = new RelayCommand(ConnectToServer, CanButtonClick);
            RegisterCommand = new RelayCommand(RegisterToServer, CanButtonClick);
            InviteForConversationCommand = new RelayCommand(InviteForConversation, CanButtonClick);
            SendConversationMessageCommand = new RelayCommand(SendConversationMessage, CanButtonClick);
            BackToMenuCommand = new RelayCommand(BackToMenu, CanButtonClick);
            KeyDownCommand = new RelayCommand<KeyEventArgs>(HandleKeyDown, CanButtonClick);

            ConversationMessages = new ObservableCollection<ConversationMessage>();

            CurrentControl = new ucConnect();

            Username = "v1.0.0";

            _window = window;

            MinimizeCommand = new RelayCommand(_ => _window.WindowState = WindowState.Minimized);
            MaximizeRestoreCommand = new RelayCommand(_ =>
            {
                _window.WindowState = _window.WindowState == WindowState.Normal
                    ? WindowState.Maximized
                    : WindowState.Normal;
            });
            CloseCommand = new RelayCommand(async _ =>
            {
                var result = await ((MainWindow)_window).ShowMessageBox("Exit", "Are you sure you want to exit?");

                if (result)
                {
                    if (IsConnected)
                    {
                        Disconnect(null); // send disconnect message only if actually connected
                    }
                    _window.Close();
                }
            });

        }

        /// <summary>
        /// Method to connect to the server and listen to updates
        /// </summary>
        /// <param name="sender"></param>
        private async void ConnectToServer(object sender)
        {
            try
            {
                UserSettings.Username = Username;
                _client = new TcpClient("127.0.0.1", 5000);
                _writer = new StreamWriter(_client.GetStream(), Encoding.UTF8) { AutoFlush = true };

                //Connect with the server
                var isConnected = await ServerHelper.ConnectToServer(_client, _writer, Username, Password);

                //If failed
                if (!isConnected)
                {
                    //Show the client
                    MessageBox.Show("Be sure to enter a valid username and password.");
                }
                else
                {
                    //Listen for updates
                    ServerHelper.ListenForUpdates(_client, this, _writer);

                    //Navigate to the main screen
                    CurrentControl = new ucMainScreen();
                }
            }
            catch (Exception)
            {
                //Show a message box instead of crashing
                MessageBox.Show("Something went wrong while connecting to the server.");
            }
        }

        private async void RegisterToServer(object sender)
        {
            try
            {
                UserSettings.Username = Username;
                _client = new TcpClient("127.0.0.1", 5000);
                _writer = new StreamWriter(_client.GetStream(), Encoding.UTF8) { AutoFlush = true };

                //Connect with the server
                var isRegistered = await ServerHelper.RegisterToServer(_client, _writer, RegisterUsername, RegisterPassword);

                //If failed
                if (!isRegistered)
                {
                    //Show the client
                    MessageBox.Show("Choose another username");
                }
                else
                {
                    MessageBox.Show("You have been successfully registerd. You can now login.");
                }
            }
            catch (Exception)
            {
                //Show a message box instead of crashing
                MessageBox.Show("Something went wrong while connecting to the server.");
            }
        }

        private void HandleKeyDown(KeyEventArgs e)
        {
            if (e.Source is TextBox textBox)
            {
                if (e.Key == Key.Enter)
                {
                    SendConversationMessage(null);
                }
            }
        }

        private async void Disconnect(object sender)
        {
            //Send to the server a message that we want to chat with the selected user
            ServerHelper.Disconnect(_client, _writer);
        }

        private async void InviteForConversation(object sender)
        {
            //Send to the server a message that we want to chat with the selected user
            ServerHelper.InviteForConversation(_client, _writer, SelectedUser);

            //Show a popup to current client with information
            MessageBox.Show($"Waiting for {_selectedUser.Username} to accept the invite");
        }

        private async void BackToMenu(object sender)
        {
            //Send to the server a message that we want to chat with the selected user
            ServerHelper.EndConversation(_client, _writer, ConversationId, UserId);

            CurrentControl = new ucMainScreen();
        }

        private async void SendConversationMessage(object sender)
        {
            if (!string.IsNullOrWhiteSpace(_message))
            {
                //Add to conversation for current client
                ConversationMessages.Add(new ConversationMessage
                {
                    Message = _message,
                    SentOn = DateTime.UtcNow,
                    SentBy = new User
                    {
                        Username = UserSettings.Username,
                    }
                });

                //Send to the server a message that we want to chat with the selected user
                ServerHelper.SendConversationMessage(_client, _writer, _message, ConversationId, UserId);

                ConvoMessage = string.Empty;
            }
        }

        private bool CanButtonClick(object parameter)
        {
            // Return true or false depending on whether the button should be enabled
            return true;
        }
    }
}
