using QuickChatter.Client.Helpers;
using QuickChatter.Client.Views.Controls;
using QuickChatter.Models;
using QuickChatter.Models.Settings;
using System.Collections.ObjectModel;
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
        private ObservableCollection<User> _onlineUsers;

        public ObservableCollection<User> OnlineUsers
        {
            get => _onlineUsers;
            set => SetProperty(ref _onlineUsers, value);
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

        //Connect Click Event
        public ICommand ConnectCommand { get; }

        //Invite user for conversation Event
        public ICommand InviteForConversationCommand { get; }

        //Constructor
        public vmMainWindow()
        {
            ConnectCommand = new RelayCommand(ConnectToServer, CanButtonClick);
            InviteForConversationCommand = new RelayCommand(InviteForConversation, CanButtonClick);

            CurrentControl = new ucConnect();

            Username = "Pinokkio";
            UserSettings.Username = "Pinokkio";

            Conversation = new Conversation
            {
                Accepter = new ConnectedClient
                {
                    Client = null,
                    Id = Guid.NewGuid(),
                    Ip = "127.0.0.1",
                    IsAvailable = false,
                    Username = "Pinokkio"
                },
                Inviter = new ConnectedClient
                {
                    Client = null,
                    Id = Guid.NewGuid(),
                    Ip = "127.0.0.1",
                    IsAvailable = false,
                    Username = "Roodkapje"
                },

                IsAccepted = true,
                Messages = new List<ConversationMessage>
                {
                    new ConversationMessage
                    {
                        Message = "Test 1ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff",
                        SentBy = new ConnectedClient
                        {
                            Client = null,
                            Id = Guid.NewGuid(),
                            Ip = "127.0.0.1",
                            IsAvailable = false,
                            Username = "Pinokkio"
                        },
                        SentOn = DateTime.UtcNow,
                    },
                    new ConversationMessage
                    {
                        Message = "Test 2",
                        SentBy = new ConnectedClient
                        {
                            Client = null,
                            Id = Guid.NewGuid(),
                            Ip = "127.0.0.1",
                            IsAvailable = false,
                            Username = "Roodkapje"
                        },
                        SentOn = DateTime.UtcNow,
                    }
                }
            };

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
                CurrentControl = new ucConversation();
            }
        }

        private async void InviteForConversation(object sender)
        {
            //Send to the server a message that we want to chat with the selected user
            ServerHelper.InviteForConversation(_client, _writer, SelectedUser);

            //Show a popup to current client with information
            MessageBox.Show($"Waiting for {_selectedUser.Username} to accept the invite");
        }

        private bool CanButtonClick(object parameter)
        {
            // Return true or false depending on whether the button should be enabled
            return true;
        }
    }
}
