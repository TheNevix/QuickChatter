using Newtonsoft.Json;
using QuickChatter.Client.ViewModels;
using QuickChatter.Client.Views.Controls;
using QuickChatter.Models;
using QuickChatter.Models.Settings;
using QuickChatter.Server.Settings;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Windows;

namespace QuickChatter.Client.Helpers
{
    public static class ServerHelper
    {
        /// <summary>
        /// Helper method to connect to the server
        /// </summary>
        /// <param name="client">The client</param>
        /// <param name="writer">The writer</param>
        /// <param name="username">The username that the client entered</param>
        /// <returns>Either true if successfully connected, false if an error occured.</returns>
        public static async Task<bool> ConnectToServer(TcpClient client, StreamWriter writer, string username, string password)
        {
            try
            {
                //Connect to the server
                await writer.WriteLineAsync($"{RequestCode.Connect}|{username}|{password}");

                var reader = new StreamReader(client.GetStream(), Encoding.UTF8);
                string? response = await reader.ReadLineAsync();

                if (response == "LOGIN_SUCCESS")
                {
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static async Task<bool> RegisterToServer(TcpClient client, StreamWriter writer, string username, string password)
        {
            try
            {
                //Register to the server
                await writer.WriteLineAsync($"{RequestCode.Register}|{username}|{password}");

                var reader = new StreamReader(client.GetStream(), Encoding.UTF8);
                string? response = await reader.ReadLineAsync();

                if (response == "REGISTER_SUCCESS")
                {
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static async Task<bool> Disconnect(TcpClient client, StreamWriter writer)
        {
            try
            {
                //Get the Ip of the client
                string userIp = IpHelpers.GetLocalIPAddress();

                //Check if null
                if (string.IsNullOrEmpty(userIp))
                {
                    return false;
                }

                //Connect to the server
                await writer.WriteLineAsync($"{RequestCode.Disconnect}");

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Invite for conversation
        /// </summary>
        /// <param name="client">The client</param>
        /// <param name="writer">The writer</param>
        /// <param name="username">The username that the client entered</param>
        /// <returns>Either true if successfully connected, false if an error occured.</returns>
        public static async Task<bool> InviteForConversation(TcpClient client, StreamWriter writer, User user)
        {
            try
            {
                //Send info to the server to invite an user for a conversation
                await writer.WriteLineAsync($"{RequestCode.InviteForConversation}|{user.Username}");

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Invite for conversation
        /// </summary>
        /// <param name="client">The client</param>
        /// <param name="writer">The writer</param>
        /// <param name="username">The username that the client entered</param>
        /// <returns>Either true if successfully connected, false if an error occured.</returns>
        public static async Task<bool> AcceptConversationInvite(TcpClient client, StreamWriter writer, string conversationId)
        {
            try
            {
                //Send info to the server to invite an user for a conversation
                await writer.WriteLineAsync($"{RequestCode.AcceptConversationInvite}|{conversationId}|{UserSettings.Username}");

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Invite for conversation
        /// </summary>
        /// <param name="client">The client</param>
        /// <param name="writer">The writer</param>
        /// <param name="username">The username that the client entered</param>
        /// <returns>Either true if successfully connected, false if an error occured.</returns>
        public static async Task<bool> SendConversationMessage(TcpClient client, StreamWriter writer, string message, string conversationId, string userId)
        {
            try
            {
                //Send info to the server to invite an user for a conversation
                await writer.WriteLineAsync($"{RequestCode.SendConversationMessage}|{conversationId}|{userId}|{message}");

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Invite for conversation
        /// </summary>
        /// <param name="client">The client</param>
        /// <param name="writer">The writer</param>
        /// <param name="username">The username that the client entered</param>
        /// <returns>Either true if successfully connected, false if an error occured.</returns>
        public static async Task<bool> EndConversation(TcpClient client, StreamWriter writer, string conversationId, string userId)
        {
            try
            {
                //Send info to the server to invite an user for a conversation
                await writer.WriteLineAsync($"{RequestCode.EndConversation}|{conversationId}|{userId}");

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static async Task ListenForUpdates(TcpClient client, vmMainWindow vm, StreamWriter writer)
        {
            try
            {
                using var reader = new StreamReader(client.GetStream(), Encoding.UTF8);
                while (true)
                {
                    string message = await reader.ReadLineAsync();
                    if (message == null)
                    {
                        break;
                    }
                    else
                    {
                        var parts = message.Split('|');

                        if (parts[0] == ResponseCode.InviteReceived)
                        {
                            var result = MessageBox.Show(parts[2], "You have been invited", MessageBoxButton.YesNo, MessageBoxImage.Question);

                            if (result == MessageBoxResult.Yes) 
                            {
                                //Send invite accepted
                                AcceptConversationInvite(client, writer, parts[1]);
                            }
                        }
                        else if (parts[0] == ResponseCode.Connected)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                vm.OnlineUsers = JsonConvert.DeserializeObject<ObservableCollection<User>>(parts[1]);
                                vm.UserId = parts[2];
                            });
                            var e = 5;
                        }
                        else if (parts[0] == ResponseCode.DisonnectedUser)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                var id = Guid.Parse(parts[1]);
                                var userToRemove = vm.OnlineUsers.FirstOrDefault(x => x.Id == id);
                                if (userToRemove != null)
                                {
                                    vm.OnlineUsers.Remove(userToRemove);
                                }
                            });
                            var e = 5;
                        }
                        else if (parts[0] == ResponseCode.ConnectedUser)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                vm.OnlineUsers.Add(JsonConvert.DeserializeObject<User>(parts[1]));
                            });
                        }
                        else if (parts[0] == ResponseCode.UpdatedUsers)
                        {
                            var updatedUsers = JsonConvert.DeserializeObject<ObservableCollection<User>>(parts[1]);
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                foreach (var updatedUser in updatedUsers)
                                {
                                    vm.OnlineUsers.First(u => u.Id == updatedUser.Id).IsAvailable = updatedUser.IsAvailable;
                                }
                            });
                        }
                        else if (parts[0] == ResponseCode.AcceptedInvite)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                vm.ConversationId = parts[1];
                                if (vm.SelectedUser == null)
                                {
                                    vm.SelectedUser = new User();
                                }
                                vm.SelectedUser.Username = parts[2];
                                vm.CurrentControl = new ucConversation();
                            });
                        }
                        else if (parts[0] == ResponseCode.ReceivedConversationMessage)
                        {
                            var convoMessage = JsonConvert.DeserializeObject<ConversationMessage>(parts[1]);

                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                vm.ConversationMessages.Add(convoMessage);
                            });
                        }
                        else if (parts[0] == ResponseCode.ConversationEnded)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                vm.CurrentControl = new ucMainScreen();
                            });

                            Task.Run(() =>
                            {
                                MessageBox.Show(parts[1]);
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var e = ex;
            }
        }
    }
}
