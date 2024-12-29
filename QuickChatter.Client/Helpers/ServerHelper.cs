﻿using Newtonsoft.Json;
using QuickChatter.Client.ViewModels;
using QuickChatter.Models;
using QuickChatter.Models.Settings;
using QuickChatter.Server.Settings;
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
        public static async Task<bool> ConnectToServer(TcpClient client, StreamWriter writer, string username)
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
                await writer.WriteLineAsync($"{RequestCode.Connect}|{username}|{userIp}");

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

        public static async Task ListenForUpdates(TcpClient client, vmMainWindow vm)
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
                            var result = MessageBox.Show(parts[1], "You have been invited", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        }
                        else if (parts[0] == ResponseCode.Connected)
                        {
                            vm.OnlineUsers = JsonConvert.DeserializeObject<List<User>>(parts[1]);
                            var e = 5;
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