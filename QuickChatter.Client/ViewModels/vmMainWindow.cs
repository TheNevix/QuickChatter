using QuickChatter.Client.Helpers;
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

        //Invite user for conversation Event
        public ICommand InviteForConversationCommand { get; }

        //Send conversation message
        public ICommand SendConversationMessageCommand { get; }
        //Back to menu/End conversation
        public ICommand BackToMenuCommand { get; }

        public ICommand KeyDownCommand { get; }

        //Constructor
        public vmMainWindow()
        {
            ConnectCommand = new RelayCommand(ConnectToServer, CanButtonClick);
            InviteForConversationCommand = new RelayCommand(InviteForConversation, CanButtonClick);
            SendConversationMessageCommand = new RelayCommand(SendConversationMessage, CanButtonClick);
            BackToMenuCommand = new RelayCommand(BackToMenu, CanButtonClick);
            KeyDownCommand = new RelayCommand<KeyEventArgs>(HandleKeyDown, CanButtonClick);

            ConversationMessages = new ObservableCollection<ConversationMessage>();

            CurrentControl = new ucConnect();
            SelectedUser = new User();

            Username = "eg. TheLegend27";
        }

        /// <summary>
        /// Method to connect to the server and listen to updates
        /// </summary>
        /// <param name="sender"></param>
        private async void ConnectToServer(object sender)
        {
            UserSettings.Username = Username;
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
                ServerHelper.ListenForUpdates(_client, this, _writer);

                //Navigate to the main screen
                CurrentControl = new ucMainScreen();
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
